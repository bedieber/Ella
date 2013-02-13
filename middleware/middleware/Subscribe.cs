using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ella.Attributes;
using Ella.Internal;
using Ella.Data;
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
        /// Subscribes the <paramref name="subscriberInstance" /> to any event matching <typeparamref name="T" /> as event data type
        /// </summary>
        /// <typeparam name="T">The type to subscribe to</typeparam>
        /// <param name="subscriberInstance">The instance of a subscriber to be subscribed to the event</param>
        /// <param name="newDataCallback">A callback method acceptiong <typeparamref name="T" /> as argument, which will be called when new data from publishers is available</param>
        /// <param name="policy">The data modify policy, default is <see cref="DataModifyPolicy.NoModify" /></param>
        /// <param name="evaluateTemplateObject">Pass a Func to subscribe using template objects. If no Func is given, <paramref name="subscriberInstance" /> will be subscribed to every event with matching <typeparamref name="T" />.<br />
        /// As an alternative, a <paramref name="evaluateTemplateObject" /> can be provided to request templates for the data to be published from every single publisher.</param>
        /// <param name="forbidRemote">if set to <c>true</c> no remote publishers will be considered.</param>
        /// <param name="subscriptionCallback">A callback method used to notify the subscriber of a new subscription. It passes a <seealso cref="SubscriptionHandle"/> instance used to identify the 1:1 relation between one publisher event and one subscriber</param>
        /// <exception cref="System.ArgumentException">subscriberInstance must be a valid subscriber</exception>
        public static void To<T>(object subscriberInstance, Action<T, SubscriptionHandle> newDataCallback, DataModifyPolicy policy = DataModifyPolicy.NoModify, Func<T, bool> evaluateTemplateObject = null, bool forbidRemote = false, Action<Type, SubscriptionHandle> subscriptionCallback = null)
        {
            _log.DebugFormat("Subscribing {0} to type {1} {2}", subscriberInstance, typeof(T),
                             (evaluateTemplateObject != null ? "with template object" : string.Empty));

            EllaModel.Instance.AddActiveSubscriber(subscriberInstance);
            if (!forbidRemote)
            {
                if (NetworkController.IsRunning)
                {
                    //Stub s = new Stub { DataType = typeof(T) };
                    //Event e = new Event { Publisher = s, EventDetail = new Attributes.PublishesAttribute(typeof(T), 1) };
                    //Start.Publisher(s);
                    //EllaModel.Instance.Subscriptions.Add(new Subscription<T>(subscriberInstance, e, newDataCallback));
                    Func<T, bool> eval = evaluateTemplateObject;
                    Action<RemoteSubscriptionHandle> callback =
                        handle => ToRemotePublisher<T>(handle, subscriberInstance, newDataCallback, policy,
                                                       eval, subscriptionCallback);
                    NetworkController.SubscribeToRemoteHost<T>(callback);
                }
            }
            if (evaluateTemplateObject == null)
                evaluateTemplateObject = (o => true);

            DoLocalSubscription(subscriberInstance, newDataCallback, evaluateTemplateObject, subscriptionCallback);

        }

        /// <summary>
        /// Does the local subscription.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriberInstance">The subscriber instance.</param>
        /// <param name="newDataCallback">The new data callback.</param>
        /// <param name="evaluateTemplateObject">The evaluate template object.</param>
        /// <param name="subscriptionCallback">The subscription callback.</param>
        /// <exception cref="System.ArgumentException">subscriberInstance must be a valid subscriber</exception>
        private static void DoLocalSubscription<T>(object subscriberInstance, Action<T, SubscriptionHandle> newDataCallback, Func<T, bool> evaluateTemplateObject, Action<Type, SubscriptionHandle> subscriptionCallback)
        {
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
                    object templateObject = Create.TemplateObject(m.Publisher, m.EventDetail.ID);
                    T template = templateObject != null ? (T)templateObject : default(T);
                    //SubscriptionID in the handle is set automatically when assigning it to a subscription
                    SubscriptionHandle handle = new SubscriptionHandle
                    {
                        EventID = m.EventDetail.ID,
                        PublisherId = EllaModel.Instance.GetPublisherId(m.Publisher),
                        SubscriberId = EllaModel.Instance.GetSubscriberId(subscriberInstance)
                    };
                    
                    var subscription = new Subscription
                    {
                        Event = m,
                        Subscriber = subscriberInstance,
                        CallbackMethod = newDataCallback.Method,
                        CallbackTarget = newDataCallback.Target,
                        Handle = handle
                    };
                    if (templateObject == null || evaluateTemplateObject(template))
                    {
                        if (!EllaModel.Instance.Subscriptions.Contains(subscription))
                        {
                            _log.InfoFormat("Subscribing {0} to {1} for type {2}", subscriberInstance, m.Publisher,
                                            m.EventDetail.DataType);
                            EllaModel.Instance.Subscriptions.Add(subscription);
                            if (subscriptionCallback != null)
                            {
                                subscriptionCallback(typeof(T), subscription.Handle);
                            }
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
        /// <param name="nodeId">The node id of the remote node.</param>
        /// <param name="subscriberAddress">The subscriber address.</param>
        /// <param name="subscriptionReference">The subscription reference.</param>
        /// <returns>An enumerable of <seealso cref="RemoteSubscriptionHandle"/> containing all new subscriptions for this object</returns>
        internal static IEnumerable<RemoteSubscriptionHandle> RemoteSubscriber(Type type, int nodeId, IPEndPoint subscriberAddress, int subscriptionReference)
        {

            _log.DebugFormat("Performing remote subscription for type {0}", type);
            var matches = EllaModel.Instance.ActiveEvents.FirstOrDefault(g => g.Key == type);


            if (matches != null)
            {
                List<RemoteSubscriptionHandle> handles = new List<RemoteSubscriptionHandle>();
                foreach (var match in matches)
                {
                    var proxy = new Proxy() { EventToHandle = match, TargetNode = subscriberAddress };
                    EllaModel.Instance.AddActiveSubscriber(proxy);
                    RemoteSubscriptionHandle handle = new RemoteSubscriptionHandle
                        {
                            EventID = match.EventDetail.ID,
                            PublisherId = EllaModel.Instance.GetPublisherId(match.Publisher),
                            SubscriberNodeID = nodeId,
                            PublisherNodeID = EllaConfiguration.Instance.NodeId,
                            SubscriberId = EllaModel.Instance.GetSubscriberId(proxy),
                            SubscriptionReference = subscriptionReference
                        };
                    //SubscriptionBase subscription = ReflectionUtils.CreateGenericSubscription(type, match, proxy);
                    SubscriptionBase subscription = new Subscription(proxy, match, proxy.GetType().GetMethod("HandleEvent"), proxy);
                    subscription.Handle = handle;
                    _log.InfoFormat("Subscribing remote subscriber to {0} for type {1}", match.Publisher,
                                    match.EventDetail.DataType);
                    EllaModel.Instance.Subscriptions.Add(subscription);
                    handles.Add(handle);
                }
                return handles;
            }
            return null;
        }

        /// <summary>
        /// Performs a subscription to a remote publisher for a local subscriber<br />#
        /// In this method, the remote subscription is completed
        /// </summary>
        private static void ToRemotePublisher<T>(RemoteSubscriptionHandle handle, object subscriberInstance, Action<T, SubscriptionHandle> newDataCallBack, DataModifyPolicy policy, Func<T, bool> evaluateTemplateObject, Action<Type, SubscriptionHandle> subscriptionCallback)
        {
            _log.DebugFormat("Completing subscription to remote publisher {0} on node {1},handle: {2}",
                             handle.PublisherId, handle.PublisherNodeID, handle);
            //TODO template object

            //Create a stub
            Stub<T> s = new Stub<T> { DataType = typeof(T), Handle = handle };
            Start.Publisher(s);
            Event ev = new Event
                {
                    Publisher = s,
                    EventDetail = (PublishesAttribute)s.GetType().GetCustomAttributes(typeof(PublishesAttribute), false).First()
                };
            handle.SubscriberId = EllaModel.Instance.GetSubscriberId(subscriberInstance);
            Subscription sub = new Subscription(subscriberInstance, ev, newDataCallBack.Method, newDataCallBack.Target) { Handle = handle };
            EllaModel.Instance.Subscriptions.Add(sub);
            if (subscriptionCallback != null)
            {
                subscriptionCallback(typeof(T), sub.Handle);
            }
        }
    }
}
