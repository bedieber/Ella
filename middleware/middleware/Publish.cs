using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ella.Exceptions;
using Ella.Model;
using log4net;

namespace Ella
{
    /// <summary>
    /// Facade class for publishing events
    /// </summary>
    public static class Publish
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Publish));

        /// <summary>
        /// This method is called to publish a new event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventData">The event data to be delivered to subscribers.</param>
        /// <param name="publisher">The publisher publishing the event.</param>
        /// <param name="eventId">The publisher-internal event ID associated with this event </param>
        /// <exception cref="InvalidPublisherException"></exception>
        public static void Event<T>(T eventData, object publisher, int eventId)
        {
            _log.DebugFormat("{0} publishes {1} for event {2}", publisher, eventData, eventId);
            if (Is.Publisher(publisher.GetType()))
            {
                //check if this one was started before
                if(!EllaModel.Instance.ActivePublishers.Contains(publisher))
                {
                    _log.ErrorFormat("Publisher {0} is not in the list of active publishers", publisher);
                    throw new StateException("Publisher is not in the list of active publishers");
                }
                else
                {
                    //TODO this is performance critical, make sure below LINQ query is suitable for that
                    /*
                     * Check if event ID matches
                     */
                    IEnumerable<Subscription<T>> subscriptions = from s in EllaModel.Instance.Subscriptions let s1 = s as Subscription<T> where s1 != null && s1.Event.EventDetail.ID==eventId select s1;
                    /*
                     * Data modification and data policies
                     * No copy: publisher has DataCopyPolicy.None && All subscribers have DataModificationPolicy.NoModify
                     * Copy once: Publisher has DataCopyPolicy.Copy && All subscribers have DataModificationPolicy.NoModify
                     * Copy n times: Publisher has DataCopyPolicy.Copy && Some of n subscribers have DataModificationPolicy.Modify
                     */
                    foreach (var subscription in subscriptions)
                    {
                        //TODO async, decide for threadpool or dedicated thread
                        subscription.Callback(eventData);
                    }
                }
            }
            else
            {
                _log.ErrorFormat("{0} is not a publisher and may not publish any events", publisher.GetType());
                throw new InvalidPublisherException(string.Format("{0} is not a publisher and may not publish any events", publisher.GetType()));
            }
        }
    }
}
