using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ella.Data;
using Ella.Model;
using Ella.Network;
using log4net;

namespace Ella
{
    /// <summary>
    /// This class allows unsubscribing from existing subscriptions.
    /// </summary>
    public static class Unsubscribe
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Subscribe));


        /// <summary>
        /// Unsubscribes the <paramref name="subscriberInstance" /> from type <typeparamref name="T" />
        /// </summary>
        /// <typeparam name="T">The type to unsubscribe from</typeparam>
        /// <exception cref="System.ArgumentException">subscriberInstance must be a valid subscriber</exception>
        public static void From<T>(object subscriberInstance)
        {
            _log.DebugFormat("Unsubscribing {0} from type {1}", EllaModel.Instance.GetSubscriberId(subscriberInstance), typeof(T));


            if (!Is.Subscriber(subscriberInstance.GetType()))
            {
                _log.ErrorFormat("Cannot unsubscribe. {0} is not a valid subscriber", subscriberInstance.GetType().ToString());
                throw new ArgumentException("subscriberInstance must be a valid subscriber");
            }

            PerformUnsubscribe(s => s.Subscriber == subscriberInstance && s.Event.EventDetail.DataType == typeof(T));
        }

        /// <summary>
        /// Unsubscribes the <paramref name="subscriberInstance" /> by SubscriptionHandle <paramref name="handle" />
        /// </summary>
        /// <param name="subscriberInstance">The instance of a subscriber to be unsubscribed</param>
        /// <param name="handle"> The SubscriptionHandle by which subscriptions should be removed </param>
        /// <exception cref="System.ArgumentException">subscriberInstance must be a valid subscriber</exception>
        public static void From(object subscriberInstance, SubscriptionHandle handle)
        {
            _log.DebugFormat("Unsubscribing {0} from handle {1}", EllaModel.Instance.GetSubscriberId(subscriberInstance), handle);


            if (!Is.Subscriber(subscriberInstance.GetType()))
            {
                _log.ErrorFormat("Cannot unsubscribe. {0} is not a valid subscriber", subscriberInstance.GetType().ToString());
                throw new ArgumentException("subscriberInstance must be a valid subscriber");
            }

            PerformUnsubscribe(s => s.Handle == handle);
        }

        /// <summary>
        /// Unsubscribes the <paramref name="subscriberInstance"/> from all events
        /// </summary>
        /// <param name="subscriberInstance">The subscriber instance.</param>
        public static void From(object subscriberInstance)
        {
            _log.DebugFormat("Unsubscribing {0} from all events", EllaModel.Instance.GetSubscriberId(subscriberInstance));


            if (!Is.Subscriber(subscriberInstance.GetType()))
            {
                _log.ErrorFormat("Cannot unsubscribe. {0} is not a valid subscriber", subscriberInstance.GetType().ToString());
                throw new ArgumentException("subscriberInstance must be a valid subscriber");
            }
            PerformUnsubscribe(s => s.Subscriber == subscriberInstance);
        }

        /// <summary>
        /// Performs the system-internal unsubscribe consisting of canelling all matching subscription according to <paramref name="selector"/> and also finding and cancelling remote subscriptions
        /// </summary>
        /// <param name="selector">The selector.</param>
        internal static void PerformUnsubscribe(Func<SubscriptionBase, bool> selector, bool performRemoteUnsubscribe = true)
        {
            var remoteSubscriptions = EllaModel.Instance.Subscriptions.Where(selector).Where(s => s.Handle is RemoteSubscriptionHandle);

            foreach (var remoteSubscription in remoteSubscriptions)
            {
                RemoteSubscriptionHandle handle = remoteSubscription.Handle as RemoteSubscriptionHandle;
                _log.DebugFormat("Cancelling remote subscription to {0}", handle);
                if (performRemoteUnsubscribe)
                    NetworkController.Unsubscribe(handle.SubscriptionReference, handle.PublisherNodeID);
                Stop.Publisher(remoteSubscription.Event.Publisher);
            }
            int removedSubscriptions = EllaModel.Instance.Subscriptions.RemoveAll(s => selector(s));

            _log.DebugFormat("{0} local subscriptions have been removed.", removedSubscriptions);
        }
    }
}
