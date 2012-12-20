using System;
using System.Linq;
using Ella.Internal;
using Ella.Model;
using Ella.Network;
using log4net;

namespace Ella
{
    /// <summary>
    /// This class allows access to subscriptions, it provides facilities to make new subscriptions
    /// </summary>
    public static class Subscribe
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Subscribe));
        /// <summary>
        /// Subscribes the <paramref name="subscriberInstance" /> to any event matching <typeparamref name="T"/> as event data type
        /// </summary>
        /// <typeparam name="T">The type to subscribe to</typeparam>
        /// <param name="subscriberInstance">The instance of a subscriber to be subscribed to the event</param>
        /// <param name="newDataCallback">A callback method acceptiong <typeparamref name="T" /> as argument, which will be called when new data from publishers is available</param>
        /// <param name="evaluateTemplateObject">Pass a Func to subscribe using template objects. If no Func is given, <paramref name="subscriberInstance"/> will be subscribed to every event with matching <typeparamref name="T"/>.<br />
        /// As an alternative, a <paramref name="evaluateTemplateObject"/> can be provided to request templates for the data to be published from every single publisher.</param>
        /// <exception cref="System.ArgumentException">subscriberInstance must be a valid subscriber</exception>
        public static void To<T>(object subscriberInstance, Action<T> newDataCallback, Func<T, bool> evaluateTemplateObject = null)
        {
            _log.DebugFormat("Subscribing {0} to type {1} {2}", subscriberInstance, typeof(T),
                             (evaluateTemplateObject != null ? "with template object" : string.Empty));
            if (evaluateTemplateObject == null)
                evaluateTemplateObject = (o => true);
            /*
             * find all matching events from currently active publishers
             * check if subscriber instace is valid subscriber
             * hold a list of subscriptions
             */
            if (!Is.Subscriber(subscriberInstance.GetType()))
            {
                _log.ErrorFormat("{0} is not a valid subscriber", subscriberInstance.GetType().ToString());
                throw new ArgumentException("subscriberInstance must be a valid subscriber");
            }
            var matches = EllaModel.Instance.ActiveEvents.FirstOrDefault(g => g.Key == typeof(T));

            if (matches != null)
            {
                _log.DebugFormat("Found {0} matches for subsription to {1}", matches.Count(), typeof(T));
                foreach (var m in matches)
                {
                    T templateObject = (T)Create.TemplateObject(m.Publisher, m.EventDetail.ID);
                    var subscription = new Subscription<T> { Event = m, Subscriber = subscriberInstance, Callback = newDataCallback };
                    if (evaluateTemplateObject(templateObject))
                    {
                        if (!EllaModel.Instance.Subscriptions.Contains(subscription))
                        {
                            _log.InfoFormat("Subscribing {0} to {1} for type {2}", subscriberInstance, m.Publisher,
                                             m.EventDetail.DataType);
                            EllaModel.Instance.Subscriptions.Add(subscription);
                        }
                    }
                    else
                        _log.DebugFormat("Templateobject from {0} was rejected by {1}", m.Publisher, subscriberInstance);
                }
            }
        }


        /// <summary>
        /// Performs a local subscription for a remote subscriber
        /// </summary>
        /// <param name="type">The type to subscribe to.</param>
        internal static void RemoteSubscriber(Type type)
        {
            _log.DebugFormat("Performing remote subscription for type {0}", type);
            var matches = EllaModel.Instance.ActiveEvents.FirstOrDefault(g => g.Key == type);


            if (matches != null)
                foreach (var match in matches)
                {
                    //TODO endpoint
                    var proxy = new Proxy() { EventToHandle = match };

                    SubscriptionBase subscription = ReflectionUtils.CreateGenericSubscription(type, match, proxy);
                    _log.InfoFormat("Subscribing remote subscriber to {0} for type {1}", match.Publisher,
                              match.EventDetail.DataType);
                    EllaModel.Instance.Subscriptions.Add(subscription);
                }
        }
    }
}
