using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ella.Internal;
using Ella.Model;

namespace Ella
{
    /// <summary>
    /// This facade class is used to indicate event associations
    /// </summary>
    public static class Associate
    {
        /// <summary>
        /// This method associates an event consumed by a publisher and an event published by it<br />
        /// This is used to indicate that the event published by the <paramref name="publisher"/> is based on (or otherwise related to) the data consumed in the <paramref name="incoming"/> event
        /// </summary>
        /// <param name="incoming">The incoming.</param>
        /// <param name="outgoingEventId">The outgoing ID.</param>
        /// <param name="publisher">The publisher.</param>
        public static void Events(SubscriptionHandle incoming, int outgoingEventId, object publisher)
        {
            
        }

        /// <summary>
        /// This method associates two events published by one publisher to indicate that those events have a semantic connection.<br />
        /// This method does not regard the order of the events (i.e. exchanging <paramref name="firstEventId"/> and <paramref name="secondEventId"/> yields no different results
        /// </summary>
        /// <param name="firstEventId">The id of the first event</param>
        /// <param name="secondEventId">The id of the second event</param>
        /// <param name="publisher">The publisher.</param>
        public static void Events(int firstEventId, int secondEventId, object publisher)
        {
            if (Is.Publisher(publisher.GetType()))
            {
                EventHandle first = new EventHandle()
                    {
                        EventId = firstEventId,
                        PublisherId = EllaModel.Instance.GetPublisherId(publisher),
                        PublisherNodeId = EllaConfiguration.Instance.NodeId
                    };
                EventHandle second = new EventHandle()
                    {
                        EventId = secondEventId,
                        PublisherId = first.PublisherId,
                        PublisherNodeId = EllaConfiguration.Instance.NodeId
                    };
                EllaModel.Instance.AddEventCorrelation(first, second);    
            }
        }
    }
}
