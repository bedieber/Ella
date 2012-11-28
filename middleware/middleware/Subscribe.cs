using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ella.Attributes;
using Ella.Model;

namespace Ella
{
    /// <summary>
    /// This class allows access to subscriptions, it provides facilities to make new subscriptions
    /// </summary>
    public static class Subscribe
    {

        /// <summary>
        /// Subscribes the <paramref name="subscriberInstance"/> to any event matching <paramref name="dataType"/> as event data type
        /// </summary>
        /// <param name="dataType">The data type to match for this event</param>
        /// <param name="subscriberInstance">The instance of a subscriber to be subscribed to the event</param>
        public static void To(Type dataType, object subscriberInstance)
        {
            /*
             * find all matching events from currently active publishers
             * check if subscriber instace is valid subscriber
             * hold a list of subscriptions
             */
            if (!Is.Subscriber(subscriberInstance.GetType()))
            {
                throw new ArgumentException("subscriberInstance must be a valid subscriber");
            }
            var matches = EllaModel.Instance.ActiveEvents.FirstOrDefault(g => g.Key==dataType);
            if (matches != null)
            {
                foreach (var m in matches)
                {
                    //TODO avoid double subscriptions
                    EllaModel.Instance.Subscriptions.Add(new Subscription { Event = m, Subscriber = subscriberInstance });
                }
            }
        }
    }
}
