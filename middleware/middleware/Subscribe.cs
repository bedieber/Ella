using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Ella.Attributes;
using Ella.Controller;
using Ella.Exceptions;
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
                    Func<T, bool> eval = evaluateTemplateObject;
                    Action<RemoteSubscriptionHandle> callback =
                        handle => SubscriptionController.ToRemotePublisher(handle, subscriberInstance, newDataCallback, policy,
                                                       eval, subscriptionCallback);
                    NetworkController.SubscribeToRemoteHost<T>(callback);
                }
            }
            if (evaluateTemplateObject == null)
                evaluateTemplateObject = (o => true);

            SubscriptionController.DoLocalSubscription(subscriberInstance, newDataCallback, evaluateTemplateObject, subscriptionCallback);

        }
    }
}
