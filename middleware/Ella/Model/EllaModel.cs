//=============================================================================
// Project  : Ella Middleware
// File    : EllaModel.cs
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
using System.Threading;

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
        /// Gets or sets the list of pending subscription requests.
        /// </summary>
        /// <value>
        /// The subscription requests.
        /// </value>
        internal List<SubscriptionRequest> SubscriptionRequests { get; set; }

        /// <summary>
        /// List of all started publishers
        /// </summary>
        private IDictionary<Publisher, int> ActivePublishers { get; set; }

        /// <summary>
        /// List of all started subscribers
        /// </summary>
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
                IEnumerable<Event> atr = (from p in ActivePublishers.Keys select p.Events).SelectMany(i => i);
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
            ActivePublishers = new Dictionary<Publisher, int>();
            Subscriptions = new List<SubscriptionBase>();
            ActiveSubscribers = new Dictionary<object, int>();
            EventCorrelations = new Dictionary<EventHandle, List<EventHandle>>();
            PublisherThreads = new List<Thread>();
            SubscriptionRequests=new List<SubscriptionRequest>();
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

        #region Publisher/SubscriptionRequest Management

        /// <summary>
        /// Adds an active publisher.
        /// </summary>
        /// <param name="instance">The instance.</param>
        internal void AddActivePublisher(Publisher instance)
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
        internal void RemoveActivePublisher(Publisher instance)
        {
            if (instance == null)
                return;
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
            return ActivePublishers.Any(k => k.Key.Instance == publisher);
        }

        /// <summary>
        /// Gets the publisher id.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        internal int GetPublisherId(object p)
        {
            return ActivePublishers.Where(k => k.Key.Instance == p).Select(k => k.Value).DefaultIfEmpty(-1).FirstOrDefault();
        }

        /// <summary>
        /// Gets the publisher to a certain ID.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The publisher object associated with the specified <paramref name="id"/>, or <c>null</c> if no such id has been issued</returns>
        internal Publisher GetPublisher(int id)
        {
            if (ActivePublishers.Values.Contains(id))
            {
                return ActivePublishers.Where(p => p.Value == id).Select(p => p.Key).FirstOrDefault();
            }
            return null;
        }

        internal Publisher GetPublisher(object instance)
        {
            if (IsActivePublisher(instance))
                return ActivePublishers.Where(p => p.Key.Instance == instance).Select(p => p.Key).FirstOrDefault();
            return null;
        }

        internal object GetSubscriber(int id)
        {
            if (ActiveSubscribers.Values.Contains(id))
            {
                return ActiveSubscribers.Where(p => p.Value == id).Select(p => p.Key).FirstOrDefault();
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
            if (ActiveSubscribers.ContainsKey(p))
            {
                return ActiveSubscribers[p];
            }
            else
            {
                return -1;
            }

        }

        internal IEnumerable<Publisher> GetActivePublishers()
        {
            return ActivePublishers.Keys;
        }

        #endregion
    }
}
