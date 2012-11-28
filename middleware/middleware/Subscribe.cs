using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ella.Attributes;

namespace Ella
{
    public class Subscription
    {
        object _subscriber;

        public object Subscriber
        {
          get { return _subscriber; }
          set { _subscriber = value; }
        }
        Event _event;

        internal Event Event
        {
          get { return _event; }
          set { _event = value; }
        }
    }

    public static class Subscribe
    {
        public static List<Subscription> _subscriptions = new List<Subscription>();

        public static void To(Type dataType, object subscriberInstance)
        {
            /*
             * find all matching events from currently active publishers
             * hold a list of subscriptions
             */
            var matches = Middleware.Instance.ActiveEvents.Where(g=>g.Key==dataType).FirstOrDefault();
            if (matches != null)
            {
                foreach (var m in matches)
                {
                    _subscriptions.Add(new Subscription { Event = m, Subscriber = subscriberInstance });
                }
            }
        }
    }
}
