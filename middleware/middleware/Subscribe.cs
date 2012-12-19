using System;
using System.Linq;
using Ella.Internal;
using Ella.Model;
using Ella.Network;

namespace Ella
{
    /// <summary>
    /// This class allows access to subscriptions, it provides facilities to make new subscriptions
    /// </summary>
    public static class Subscribe
    {

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
            if (evaluateTemplateObject == null)
                evaluateTemplateObject = (o => true);
            /*
             * find all matching events from currently active publishers
             * check if subscriber instace is valid subscriber
             * hold a list of subscriptions
             */
            if (!Is.Subscriber(subscriberInstance.GetType()))
            {
                throw new ArgumentException("subscriberInstance must be a valid subscriber");
            }
            var matches = EllaModel.Instance.ActiveEvents.FirstOrDefault(g => g.Key == typeof(T));
            if (matches != null)
            {
                foreach (var m in matches)
                {
                    T templateObject = (T)Create.TemplateObject(m.Publisher, m.EventDetail.ID);
                    var subscription = new Subscription<T> { Event = m, Subscriber = subscriberInstance, Callback = newDataCallback };
                    if (evaluateTemplateObject(templateObject))
                        if (!EllaModel.Instance.Subscriptions.Contains(subscription))
                            EllaModel.Instance.Subscriptions.Add(subscription);
                }
            }
        }


        /// <summary>
        /// Performs a local subscription for a remote subscriber
        /// </summary>
        /// <param name="type">The type to subscribe to.</param>
        internal static void RemoteSubscriber(Type type)
        {
            var matches = EllaModel.Instance.ActiveEvents.FirstOrDefault(g => g.Key == type);


            if (matches != null)
                foreach (var match in matches)
                {
                    //TODO endpoint
                    var proxy = new Proxy() { EventToHandle = match };

                    SubscriptionBase subscription = ReflectionUtils.CreateGenericSubscription(type, match, proxy);
                    EllaModel.Instance.Subscriptions.Add(subscription);
                }
        }
    }
}
