using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ella.Data;
using Ella.Model;
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


            if(!Is.Subscriber(subscriberInstance.GetType()))
            {
                _log.ErrorFormat("Cannot unsubscribe. {0} is not a valid subscriber", subscriberInstance.GetType().ToString());
                throw new ArgumentException("subscriberInstance must be a valid subscriber");
            }

            int removedSubscriptions = EllaModel.Instance.Subscriptions.RemoveAll(s => s.Event.EventDetail.DataType == typeof (T)); 
            _log.DebugFormat("{0} subscriptions have been removed.");
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

            int removedSubscriptions = EllaModel.Instance.Subscriptions.RemoveAll(s => s.Handle == handle);
            _log.DebugFormat("{0} subscriptions have been removed.", removedSubscriptions);
        }
    }
}
