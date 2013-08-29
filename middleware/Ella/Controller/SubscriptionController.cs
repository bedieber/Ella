//=============================================================================
// Project  : Ella Middleware
// File    : SubscriptionController.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Ella.Attributes;
using Ella.Data;
using Ella.Exceptions;
using Ella.Internal;
using Ella.Model;
using Ella.Network;
using Ella.Network.Communication;
using log4net;

namespace Ella.Controller
{
    /// <summary>
    /// Handles all subscription-relevant operations
    /// </summary>
    internal class SubscriptionController
    {
        private static ILog _log = LogManager.GetLogger(typeof(SubscriptionController));

        private static IEnumerable<Proxy> ActiveProxies
        {
            get
            {
                return
                    EllaModel.Instance.Subscriptions.Where(s => s.Subscriber is Proxy)
                             .Select(s => s.Subscriber as Proxy).ToList();
            }
        }
        /// <summary>
        /// Does the local subscription.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriberInstance">The subscriber instance.</param>
        /// <param name="newDataCallback">The new data callback.</param>
        /// <param name="evaluateTemplateObject">The evaluate template object.</param>
        /// <param name="subscriptionCallback">The subscription callback.</param>
        /// <param name="policy">The modification policy.</param>
        /// <exception cref="System.ArgumentException">subscriberInstance must be a valid subscriber</exception>
        /// <exception cref="IllegalAttributeUsageException"></exception>
        internal static void DoLocalSubscription<T>(object subscriberInstance, Action<T, SubscriptionHandle> newDataCallback, Func<T, bool> evaluateTemplateObject, Action<Type, SubscriptionHandle> subscriptionCallback, DataModifyPolicy policy)
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
                Dictionary<SubscriptionHandle, SubscriptionHandle> correlatedEvents = new Dictionary<SubscriptionHandle, SubscriptionHandle>();
                MethodBase associateMethod = ReflectionUtils.GetAttributedMethod(subscriberInstance.GetType(), typeof(AssociateAttribute));

                _log.DebugFormat("Found {0} matches for subsription to {1}", matches.Count(), typeof(T));
                foreach (var m in matches)
                {
                    if (m.Publisher == subscriberInstance)
                    {
                        _log.DebugFormat("Not subscribing {0} for itself", subscriberInstance);
                        continue;
                    }
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
                            Handle = handle,
                            DataType = typeof(T),
                            ModifyPolicy = policy
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
                            NotifyPublisher(subscription.Event, subscription.Handle);
                            if (associateMethod != null)
                            {
                                var correlations = EllaModel.Instance.GetEventCorrelations(handle.EventHandle);
                                if (correlations != null)
                                {
                                    foreach (
                                        SubscriptionHandle correlationHandle in
                                            correlations.Select(correlation => new SubscriptionHandle()
                                                {
                                                    EventHandle = correlation,
                                                }))
                                    {
                                        correlationHandle.SubscriberId =
                                            handle.SubscriberId;
                                        correlatedEvents.Add(handle, correlationHandle);

                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        _log.DebugFormat("Templateobject from {0} was rejected by {1}", m.Publisher, subscriberInstance);
                    }
                }
                if (associateMethod != null)
                {
                    if (associateMethod.GetParameters().Count() != 2 || associateMethod.GetParameters().Any(p => p.ParameterType != typeof(SubscriptionHandle)))
                        throw new IllegalAttributeUsageException(String.Format("Method {0} attributed as Associate has invalid parameters (count or type)", associateMethod));
                    foreach (var handlePair in correlatedEvents)
                    {
                        //Only do this if subscriber is subscribed to both events
                        if (EllaModel.Instance.Subscriptions.Any(s =>
                                                                 Equals(s.Handle.EventHandle,
                                                                        handlePair.Value.EventHandle) &&
                                                                 s.Subscriber == subscriberInstance))
                            associateMethod.Invoke(subscriberInstance,
                                                   new object[] { handlePair.Key, handlePair.Value });
                    }
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
        internal static IEnumerable<RemoteSubscriptionHandle> SubscribeRemoteSubscriber(Type type, int nodeId, IPEndPoint subscriberAddress, int subscriptionReference)
        {
            _log.DebugFormat("Performing remote subscription for type {0}", type);
            var matches = EllaModel.Instance.ActiveEvents.FirstOrDefault(g => g.Key == type);

            if (matches != null)
            {
                List<RemoteSubscriptionHandle> handles = new List<RemoteSubscriptionHandle>();
                foreach (var match in matches)
                {
                    var proxy = match.EventDetail.NeedsReliableTransport ?
                        new Proxy()
                        : GetMulticastProxy(match);
                    proxy.EventToHandle = match;
                    proxy.IpSender = new IpSender(subscriberAddress.Address.ToString(), subscriberAddress.Port);


                    //TODO make max queue size changeable
                    if (!match.EventDetail.NeedsReliableTransport)
                        proxy.IpSender.MaxQueueSize = 50;

                    EllaModel.Instance.AddActiveSubscriber(proxy);

                    RemoteSubscriptionHandle handle = match.EventDetail.NeedsReliableTransport
                                                          ? new RemoteSubscriptionHandle()
                                                          : new MulticastRemoteSubscriptionhandle
                                                          {
                                                              IpAddress = proxy.UdpSender.TargetNode.Address.ToString(),
                                                              Port = proxy.UdpSender.TargetNode.Port
                                                          };
                    handle.EventID = match.EventDetail.ID;
                    handle.PublisherId = EllaModel.Instance.GetPublisherId(match.Publisher);
                    handle.SubscriberNodeID = nodeId;
                    handle.PublisherNodeID = EllaConfiguration.Instance.NodeId;
                    handle.SubscriberId = EllaModel.Instance.GetSubscriberId(proxy);
                    handle.SubscriptionReference = subscriptionReference;

                    _log.DebugFormat("Constructing remote subscription handle {0}", handle);
                    SubscriptionBase subscription = new Subscription(proxy, match, proxy.GetType().GetMethod("HandleEvent", BindingFlags.NonPublic | BindingFlags.Instance), proxy);
                    subscription.Handle = handle;
                    subscription.DataType = type;
                    _log.InfoFormat("Subscribing remote subscriber to {0} for type {1}", match.Publisher,
                                    match.EventDetail.DataType);
                    EllaModel.Instance.Subscriptions.Add(subscription);
                    handles.Add(handle);

                    NotifyPublisher(match, handle);
                }
                return handles;
            }
            return null;
        }


        /// <summary>
        /// Performs a callback to a publisher that a new subscriber has been added
        /// </summary>
        /// <param name="ev">The ev.</param>
        /// <param name="handle">The handle.</param>
        internal static void NotifyPublisher(Event ev, SubscriptionHandle handle)
        {
            string callback = ev.EventDetail.SubscriptionCallback;

            if (callback == null)
                return;

            MethodInfo info = ev.Publisher.GetType().GetMethod(callback);


            if (info != null)
            {
                object[] parameters = { ev.EventDetail.ID, handle };
                info.Invoke(ev.Publisher, parameters);
            }

        }

        /// <summary>
        /// Gets the multicast proxy.
        /// </summary>
        /// <param name="match">The match of type <see cref="Ella.Model.Event"/>.</param>
        /// <returns></returns>
        private static Proxy GetMulticastProxy(Event match)
        {
            Proxy sender = ActiveProxies.FirstOrDefault(p => p.EventToHandle == match) ??
                          new Proxy() { EventToHandle = match, UdpSender = new UdpSender() };
            return sender;
        }

        /// <summary>
        /// Performs a subscription to a remote publisher for a local subscriber<br />#
        /// In this method, the remote subscription is completed
        /// </summary>
        internal static void SubscribeToRemotePublisher<T>(RemoteSubscriptionHandle handle, object subscriberInstance, Action<T, SubscriptionHandle> newDataCallBack, DataModifyPolicy policy, Func<T, bool> evaluateTemplateObject, Action<Type, SubscriptionHandle> subscriptionCallback)
        {
            _log.DebugFormat("Completing subscription to remote publisher {0} on node {1},handle: {2}",
                             handle.PublisherId, handle.PublisherNodeID, handle);
            //TODO template object

            //Create a stub
            Stub<T> s = new Stub<T> { DataType = typeof(T), Handle = handle };

            var publishesAttribute = (PublishesAttribute)s.GetType().GetCustomAttributes(typeof(PublishesAttribute), false).First();

            if (handle is MulticastRemoteSubscriptionhandle)
            {
                _log.DebugFormat("{0} is potential multicast subscription, opening multicast port", handle);
                MulticastRemoteSubscriptionhandle mh = handle as MulticastRemoteSubscriptionhandle;
                Networking.ConnectToMulticast(mh.IpAddress, mh.Port);
                RemoteSubscriptionHandle h = new RemoteSubscriptionHandle()
                    {
                        PublisherNodeID = handle.PublisherNodeID,
                        PublisherId = handle.PublisherId,
                        SubscriptionReference = handle.SubscriptionReference,
                        EventID = handle.EventID,
                        SubscriberNodeID = handle.SubscriberNodeID
                    };
                s.Handle = h;
            }
            Start.Publisher(s);
            Event ev = new Event
                {
                    Publisher = s,
                    EventDetail = publishesAttribute
                };
            handle.SubscriberId = EllaModel.Instance.GetSubscriberId(subscriberInstance);
            Subscription sub = new Subscription(subscriberInstance, ev, newDataCallBack.Method, newDataCallBack.Target) { Handle = s.Handle, DataType = typeof(T) };
            EllaModel.Instance.Subscriptions.Add(sub);
            if (subscriptionCallback != null)
            {
                _log.DebugFormat("Found subscription callback for {0}, notifying subscriber", handle);
                subscriptionCallback(typeof(T), sub.Handle);
            }
            else
            {
                _log.DebugFormat("Subscription callback for {0} was null, not notifying subscriber", handle);
            }
        }


        /// <summary>
        /// Performs the system-internal unsubscribe consisting of canelling all matching subscription according to <paramref name="selector" /> and also finding and cancelling remote subscriptions
        /// </summary>
        /// <param name="selector">The selector.</param>
        /// <param name="performRemoteUnsubscribe">if set to <c>true</c> [perform remote unsubscribe].</param>
        internal static void PerformUnsubscribe(Func<SubscriptionBase, bool> selector, bool performRemoteUnsubscribe = true)
        {
            var remoteSubscriptions = EllaModel.Instance.Subscriptions.Where(selector).Where(s => s.Handle is RemoteSubscriptionHandle);

            foreach (var remoteSubscription in remoteSubscriptions)
            {
                RemoteSubscriptionHandle handle = remoteSubscription.Handle as RemoteSubscriptionHandle;
                if (handle.PublisherNodeID == EllaConfiguration.Instance.NodeId)
                    continue;
                _log.DebugFormat("Cancelling remote subscription to {0}", handle);

                if (performRemoteUnsubscribe)
                    Networking.Unsubscribe(handle.SubscriptionReference, handle.PublisherNodeID);
                Stop.Publisher(remoteSubscription.Event.Publisher);
            }
            int removedSubscriptions = EllaModel.Instance.Subscriptions.RemoveAll(s => selector(s));

            _log.DebugFormat("{0} local subscriptions have been removed.", removedSubscriptions);
        }


    }
}
