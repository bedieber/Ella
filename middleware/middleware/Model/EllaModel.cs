using System;
using System.Collections.Generic;
using System.Linq;
using Ella.Attributes;

namespace Ella.Model
{
    /// <summary>
    /// This class holds the state of Ella containing collections for known modules, active publishers and current subscriptions
    /// </summary>
    internal class EllaModel
    {
        #region internal Singleton
        private static readonly EllaModel _instance = new EllaModel();

        internal static EllaModel Instance
        {
            get { return _instance; }
        }
        #endregion

        /// <summary>
        /// Constructor for the model
        /// </summary>
        public EllaModel()
        {
            Publishers = new List<Type>();
            Subscribers = new List<Type>();
            ActivePublishers = new List<object>();
            Subscriptions=new List<SubscriptionBase>();
        }

        /// <summary>
        /// List of all known publishers
        /// </summary>
        internal ICollection<Type> Publishers
        {
            get;
            set;
        }

        /// <summary>
        /// List of all known subscriber types
        /// </summary>
        internal ICollection<Type> Subscribers { get; set; }

        internal List<SubscriptionBase> Subscriptions
        {
            get;
            set;
        }


        /// <summary>
        /// List of all started publishers
        /// </summary>
        internal ICollection<object> ActivePublishers { get; set; }



        internal IEnumerable<IGrouping<Type, Event>> ActiveEvents
        {
            get
            {
                //from all active publishers, take their subscribes attributes as one flat list
                IEnumerable<Event> atr = (from p in ActivePublishers let a = (((IEnumerable<PublishesAttribute>)p.GetType().GetCustomAttributes(typeof(PublishesAttribute), true))).Select(e => new Event { Publisher = p, EventDetail = e }) select a).SelectMany(i => i);
                //make a dictionary out of that list, where the key is the type of published data
                return atr.GroupBy(a => a.EventDetail.DataType);
                //return atr.ToLookup(a => a.EventDetail.DataType);
            }
        }
    }
}
