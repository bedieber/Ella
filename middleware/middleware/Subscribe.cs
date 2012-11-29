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
        /// Subscribes the <paramref name="subscriberInstance" /> to any event matching <paramref name="dataType" /> as event data type
        /// </summary>
        /// <typeparam name="T">The type to subscribe to</typeparam>
        /// <param name="subscriberInstance">The instance of a subscriber to be subscribed to the event</param>
        /// <param name="newDataCallback">A callback method acceptiong <typeparamref name="T"/> as argument, which will be called when new data from publishers is available</param>
        /// <exception cref="System.ArgumentException">subscriberInstance must be a valid subscriber</exception>
        public static void To<T>(object subscriberInstance, Action<T> newDataCallback )
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
            var matches = EllaModel.Instance.ActiveEvents.FirstOrDefault(g => g.Key==typeof(T));
            if (matches != null)
            {
                foreach (var m in matches)
                {
                    //TODO avoid double subscriptions
                    EllaModel.Instance.Subscriptions.Add(new Subscription<T> { Event = m, Subscriber = subscriberInstance, Callback=newDataCallback });
                }
            }
        }
    }
}
