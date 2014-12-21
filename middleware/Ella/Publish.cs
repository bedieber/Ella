//=============================================================================
// Project  : Ella Middleware
// File    : Publish.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Ella.Data;
using Ella.Exceptions;
using Ella.Internal;
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
        /// <param name="subscribers">A list of subscribers, to which the events should be published. If it is null, all subscribers get the event.</param>
        /// <exception cref="InvalidPublisherException"></exception>
        public static void Event<T>(T eventData, object publisher, int eventId, List<SubscriptionHandle> subscribers = null)
        {
            if (publisher == null)
            {
                _log.Error("NULL publisher supplied for event");
                return;
            }
            if (Is.Publisher(publisher.GetType()))
            {
                //check if this one was started before
                if (!EllaModel.Instance.IsActivePublisher(publisher))
                {
                    _log.ErrorFormat("Publisher {0} is not in the list of active publishers", publisher);
                    throw new StateException("Publisher is not in the list of active publishers");
                }
                else
                {
                    /*
                     * Check if event ID matches
                     */
                    Predicate<SubscriptionBase> checkValid =
                        (s) =>
                        {

                            if (s.DataType.FullName == typeof(T).FullName)
                                if (s.Event.Publisher == publisher)
                                    if (s.Event.EventDetail.ID == eventId)
                                        return true;
                            //if (s.Handle is RemoteSubscriptionHandle)
                            //{
                            //    _log.DebugFormat(
                            //        "not valid: types {0} {1},  publisher==publisher? {2}, event ids {3} {4}",
                            //        s.DataType.FullName, typeof(T).FullName, s.Event.Publisher == publisher, s.Event.EventDetail.ID, eventId);
                            //}
                            return false;
                        };
                    IEnumerable<Subscription> subscriptions = (from s in EllaModel.Instance.FilterSubscriptions( checkValid) select s as Subscription).ToArray();
                    /*
                     * Data modification and data policies
                     * No copy: publisher has DataCopyPolicy.None && All subscribers have DataModificationPolicy.NoModify
                     * Copy once: Publisher has DataCopyPolicy.Copy && All subscribers have DataModificationPolicy.NoModify
                     * Copy n times: Publisher has DataCopyPolicy.Copy && Some of n subscribers have DataModificationPolicy.Modify
                     */

                    T data = eventData;

                    var subscriptionsArray = subscriptions as Subscription[] ?? subscriptions.ToArray();
                    if (subscriptionsArray.Length == 0)
                    {
                        _log.DebugFormat("No subscribers found for event {0} of publisher {1}", eventId, publisher);
                        return;
                    }
                    _log.DebugFormat("{0} publishes {1} for event {2} {3}", publisher, eventData, eventId, subscribers == null ? "to " + subscriptionsArray.Length + " subscribers" : "to a subset of subscribers");

                    if (subscriptionsArray.ElementAt(0).Event.EventDetail.CopyPolicy == DataCopyPolicy.Copy)
                    {
                        data = Serializer.SerializeCopy(eventData);
                    }

                    if (subscribers == null)
                    {
                        foreach (var sub in subscriptionsArray)
                        {
                            Thread t =
                                new Thread((ThreadStart)
                                    delegate
                                    {
                                        try
                                        {
                                            sub.CallbackMethod.Invoke(sub.CallbackTarget,
                                                                                new object[]
                                            {
                                                sub.ModifyPolicy == DataModifyPolicy.Modify
                                                    ? Serializer.SerializeCopy(data)
                                                    : data,
                                                sub.Handle
                                            });
                                        }
                                        catch (Exception ex)
                                        {
                                            if (Debugger.IsAttached)
                                                Debugger.Break();
                                            do
                                            {
                                                _log.FatalFormat(
                                                    "Could not invoke callbackmethod {0} on type {1}  for subscription {2}: {3} \n{4}",
                                                    sub.CallbackMethod.Name, sub.CallbackTarget.GetType(), typeof (T),
                                                    ex.Message, ex.StackTrace);
                                                ex = ex.InnerException;
                                            } while (ex.InnerException != null);
                                        }
                                    });
                            t.Start();
                        }
                    }
                    else
                    {
                        foreach (var sub in subscriptionsArray)
                        {
                            if (subscribers.Contains(sub.Handle))
                            {
                                Thread t =
                                new Thread(
                                    () =>
                                    sub.CallbackMethod.Invoke(sub.CallbackTarget,
                                                              new object[]
                                                                  {
                                                                      sub.ModifyPolicy == DataModifyPolicy.Modify
                                                                          ? Serializer.SerializeCopy(data)
                                                                          : data,
                                                                      sub.Handle
                                                                  }));
                                t.Start();
                            }
                        }
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
