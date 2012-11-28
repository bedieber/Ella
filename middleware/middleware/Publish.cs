using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ella.Exceptions;
using Ella.Model;

namespace Ella
{
    /// <summary>
    /// Facade class for publishing events
    /// </summary>
    public static class Publish
    {
        /// <summary>
        /// This method is called to publish a new event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventData">The event data to be delivered to subscribers.</param>
        /// <param name="publisher">The publisher publishing the event.</param>
        /// <param name="eventId">The publisher-internal event ID associated with this event </param>
        /// <exception cref="InvalidPublisherException"></exception>
        public static void PublishEvent<T>(T eventData, object publisher, int eventId)
        {
            if (Is.Publisher(publisher.GetType()))
            {
                //check if this one was started before
                if(!EllaModel.Instance.ActivePublishers.Contains(publisher))
                {
                    //TODO throw an exception
                }
                else
                {
                    //TODO lookup subscribers, publish event asynchronously
                    /*
                     * Check if event ID matches
                     */
                }
            }
            else
            {
                throw new InvalidPublisherException(string.Format("{0} is not a publisher and may not publish any events", publisher.GetType()));
            }
        }
    }
}
