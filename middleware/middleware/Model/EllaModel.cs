using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Ella.Attributes;
using log4net.Config;

namespace Ella.Model
{
    /// <summary>
    /// This class holds the state of Ella containing collections for known modules, active publishers and current subscriptions
    /// </summary>
    internal class EllaModel
    {
        //TODO synchronize methods and accessors here

        private int _nextModuleID = new Random().Next(100);

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
            Reset();
        }

        public ICollection<Thread> PublisherThreads { get; set; }

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
        private IDictionary<object, int> ActivePublishers { get; set; }

        private IDictionary<object, int> ActiveSubscribers { get; set; }

        /// <summary>
        /// Gets or sets the event correlations.
        /// </summary>
        /// <value>
        /// The event correlations.
        /// </value>
        private Dictionary<EventHandle, List<EventHandle>> EventCorrelations { get; set; }

        internal IEnumerable<IGrouping<Type, Event>> ActiveEvents
        {
            get
            {
                //from all active publishers, take their publishes attributes as one flat list
                IEnumerable<Event> atr = (from p in ActivePublishers.Keys let a = (((IEnumerable<PublishesAttribute>)p.GetType().GetCustomAttributes(typeof(PublishesAttribute), true))).Select(e => new Event { Publisher = p, EventDetail = e }) select a).SelectMany(i => i);
                //make a dictionary out of that list, where the key is the type of published data
                return atr.GroupBy(a => a.EventDetail.DataType);
                //return atr.ToLookup(a => a.EventDetail.DataType);
            }
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        internal void Reset()
        {
            Publishers = new List<Type>();
            Subscribers = new List<Type>();
            ActivePublishers = new Dictionary<object, int>();
            Subscriptions = new List<SubscriptionBase>();
            ActiveSubscribers = new Dictionary<object, int>();
            EventCorrelations = new Dictionary<EventHandle, List<EventHandle>>();
            PublisherThreads = new List<Thread>();
        }

        /// <summary>
        /// Adds a new event correlation. Checks if this pair is already present and adds it if not.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        internal void AddEventCorrelation(EventHandle first, EventHandle second)
        {
            CorrelateEvents(first, second);
            CorrelateEvents(second, first);
        }

        private void CorrelateEvents(EventHandle first, EventHandle second)
        {
            if (EventCorrelations.ContainsKey(first))
            {
                if (!EventCorrelations[first].Contains(second))
                    EventCorrelations[first].Add(second);
            }
            else
            {
                EventCorrelations.Add(first, new List<EventHandle>() { second });
            }
        }

        internal IEnumerable<EventHandle> GetEventCorrelations(EventHandle handle)
        {
            if (EventCorrelations.ContainsKey(handle))
                return EventCorrelations[handle];
            return new List<EventHandle>();
        }

        #region Publisher/Subscriber Management

        /// <summary>
        /// Adds an active publisher.
        /// </summary>
        /// <param name="instance">The instance.</param>
        internal void AddActivePublisher(object instance)
        {

            if (!ActivePublishers.ContainsKey(instance))
            {
                ActivePublishers.Add(instance, Interlocked.Increment(ref _nextModuleID));
            }
        }

        /// <summary>
        /// Adds an active subscriber.
        /// </summary>
        /// <param name="instance">The instance.</param>
        internal void AddActiveSubscriber(object instance)
        {
            if (!ActiveSubscribers.ContainsKey(instance))
            {
                ActiveSubscribers.Add(instance, Interlocked.Increment(ref _nextModuleID));
            }
        }

        /// <summary>
        /// Removes the active publisher.
        /// </summary>
        /// <param name="instance">The instance.</param>
        internal void RemoveActivePublisher(object instance)
        {
            if (ActivePublishers.ContainsKey(instance))
                ActivePublishers.Remove(instance);
        }

        /// <summary>
        /// Determines whether <paramref name="publisher"/> is an active publisher.
        /// </summary>
        /// <param name="publisher">The publisher.</param>
        /// <returns>
        ///   <c>true</c> if [is active publisher] [the specified publisher]; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsActivePublisher(object publisher)
        {
            return ActivePublishers.ContainsKey(publisher);
        }

        /// <summary>
        /// Gets the publisher id.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        internal int GetPublisherId(object p)
        {
            return ActivePublishers[p];
        }

        /// <summary>
        /// Gets the publisher to a certain ID.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The publisher object associated with the specified <paramref name="id"/>, or <c>null</c> if no such id has been issued</returns>
        internal object GetPublisher(int id)
        {
            if (ActivePublishers.Values.Contains(id))
            {
                return ActivePublishers.Where(p => p.Value == id).Select(p => p.Key).FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// Gets the subscriber id.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        internal int GetSubscriberId(object p)
        {
            return ActiveSubscribers[p];
        }

        internal IEnumerable<object> GetActivePublishers()
        {
            return ActivePublishers.Keys;
        }

        #endregion
    }
}
