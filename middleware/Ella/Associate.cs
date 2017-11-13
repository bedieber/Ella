//=============================================================================
// Project  : Ella Middleware
// File    : Associate.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: https://github.com/bedieber/Ella.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using System.Linq;
using Ella.Attributes;
using Ella.Exceptions;
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

                foreach (var result in EllaModel.Instance.FilterSubscriptions(s=>true).GroupBy(s => s.Subscriber).Where(g => g.Any(g1 => Equals(g1.Handle.EventHandle, first)) && g.Any(g2 => Equals(g2.Handle.EventHandle, second))).Select(g => new { Object = g.Key, Method = ReflectionUtils.GetAttributedMethod(g.Key.GetType(), typeof(AssociateAttribute)) }))
                {
                    if (result.Method != null)
                    {
                        if (result.Method.GetParameters().Count() != 2 || result.Method.GetParameters().Any(p => p.ParameterType != typeof(SubscriptionHandle)))
                            throw new IllegalAttributeUsageException(String.Format("Method {0} attributed as Associate has invalid parameters (count or type)", result.Method));
                        SubscriptionHandle handle1 = new SubscriptionHandle() { EventHandle = first };
                        SubscriptionHandle handle2 = new SubscriptionHandle() { EventHandle = second };

                        result.Method.Invoke(result.Object, new object[] { handle1, handle2 });
                        result.Method.Invoke(result.Object, new object[] { handle2, handle1 });
                    }
                }
            }
        }
    }
}
