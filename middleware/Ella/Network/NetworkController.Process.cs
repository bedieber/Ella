//=============================================================================
// Project  : Ella Middleware
// File    : NetworkController.Process.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@uni-klu.ac.at)
// Copyright 2012 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Ella.Attributes;
using Ella.Control;
using Ella.Controller;
using Ella.Exceptions;
using Ella.Internal;
using Ella.Model;
using Ella.Network.Communication;

namespace Ella.Network
{
    internal partial class NetworkController
    {
        #region Discovery
        private void ProcessDiscover(MessageEventArgs e)
        {
            /*
             * Respond to a discovery message
             */
            int port = BitConverter.ToInt32(e.Message.Data, 4);
            IPEndPoint ep = (IPEndPoint)e.Address;
            ep.Port = port;
            if (!_remoteHosts.ContainsKey(e.Message.Sender))
            {
                _log.InfoFormat("Discovered host {0} on {1}", e.Message.Sender, ep);
                _remoteHosts.Add(e.Message.Sender, ep);
            }
            byte[] portBytes = BitConverter.GetBytes(EllaConfiguration.Instance.NetworkPort);
            //byte[] bytes = new byte[idBytes.Length + portBytes.Length];
            //Array.Copy(idBytes, bytes, idBytes.Length);
            //Array.Copy(portBytes, 0, bytes, idBytes.Length, portBytes.Length);
            Message m = new Message { Type = MessageType.DiscoverResponse, Data = portBytes };
            Client.Send(m, ep.Address.ToString(), ep.Port);

            /*
             * Send a message to the new host and inquiry about possible new publishers
             */
            //TODO a delay makes sense here because the other node may still be starting publishers when the local node already inquires. However this only covers most of an automatic startup
            //TODO think about regular inquiries to known nodes or a notification mechanism about a newly started publisher
            Thread.Sleep(2000);
            foreach (var sub in _subscriptionCache)
            {
                Message subscription = new Message(sub.Key) { Type = MessageType.Subscribe, Data = Serializer.Serialize(sub.Value) };
                Client.SendAsync(subscription, ep.Address.ToString(), ep.Port);
            }
        }

        private void ProcessDiscoverResponse(MessageEventArgs e)
        {
            if (!_remoteHosts.ContainsKey(e.Message.Sender))
            {
                int port = BitConverter.ToInt32(e.Message.Data, 0);
                IPEndPoint ep = (IPEndPoint)e.Address;
                ep.Port = port;
                _log.InfoFormat("Host {0} on {1} responded to discovery", e.Message.Sender, ep);
                _remoteHosts.Add(e.Message.Sender, ep);
            }
        }
        #endregion
        #region Subscribe

        private void ProcessSubscribeResponse(MessageEventArgs e)
        {
            if (e.Message.Data.Length > 0)
            {
                int inResponseTo = BitConverter.ToInt32(e.Message.Data, 0);
                ICollection<RemoteSubscriptionHandle> handles =
                    Serializer.Deserialize<ICollection<RemoteSubscriptionHandle>>(e.Message.Data, 4);
                //var stubs = from s in EllaModel.Instance.Subscriptions
                //            where s.Event.Publisher is Stub && (s.Event.Publisher as Stub).DataType == type
                //            select s;

                if (_pendingSubscriptions.ContainsKey(inResponseTo))
                {
                    Action<RemoteSubscriptionHandle> action = _pendingSubscriptions[inResponseTo];
                    foreach (var handle in handles)
                    {
                        handle.PublisherNodeID = e.Message.Sender;
                        action(handle);
                    }
                }
                else
                {
                    _log.WarnFormat(
                        "Detected invalid subscription reference {0}, may be from previous application run. Unsubscribing...",
                        inResponseTo);
                    //TODO this is not a clean solution, might clash with other msgIDs
                    UnsubscribeFrom(inResponseTo, e.Message.Sender);
                }
            }
        }

        private void ProcessSubscribe(MessageEventArgs e)
        {
            Type type = null;
            try
            {
                type = Serializer.Deserialize<Type>(e.Message.Data);
            }
            catch (Exception)
            {
                return;
            }
           //get the subscriptions that this node is already subscribed for, to avoid double subscriptions
            var currentHandles = (from s1 in (EllaModel.Instance.Subscriptions.Where(s => s.Event.EventDetail.DataType == type).Select(s => s.Handle)).OfType<RemoteSubscriptionHandle>()
                                  where s1.SubscriberNodeID == e.Message.Sender
                                  select s1).ToList().GroupBy(s => s.SubscriptionReference);

            IEnumerable<RemoteSubscriptionHandle> handles = SubscriptionController.SubscribeRemoteSubscriber(type, e.Message.Sender,
                                                                                       (IPEndPoint)
                                                                                       _remoteHosts[e.Message.Sender],
                                                                                       e.Message.Id);

            /*
             * Sending reply
             */
            IPEndPoint ep = (IPEndPoint)_remoteHosts[e.Message.Sender];
            if (ep == null)
            {
                _log.ErrorFormat("No suitable endpoint to reply to subscription found");
                return;
            }
            if (handles != null)
            {
                //Send reply
                byte[] handledata = Serializer.Serialize(handles);
                byte[] reply = new byte[handledata.Length + 4];
                byte[] idbytes = BitConverter.GetBytes(e.Message.Id);
                Array.Copy(idbytes, reply, idbytes.Length);
                Array.Copy(handledata, 0, reply, idbytes.Length, handledata.Length);
                Message m = new Message { Type = MessageType.SubscribeResponse, Data = reply };
                _log.DebugFormat("Replying to subscription request at {0}", ep);

                Client.Send(m, ep.Address.ToString(), ep.Port);
            }
            /* 
                 * Notify about previous subscriptions on the same type by the same node
                 */
            foreach (var currentHandle in currentHandles)
            {
                //Send reply
                byte[] handledata = Serializer.Serialize(currentHandle.ToList());
                byte[] reply = new byte[handledata.Length + 4];
                byte[] idbytes = BitConverter.GetBytes(currentHandle.Key);
                Array.Copy(idbytes, reply, idbytes.Length);
                Array.Copy(handledata, 0, reply, idbytes.Length, handledata.Length);
                Message m = new Message { Type = MessageType.SubscribeResponse, Data = reply };

                _log.DebugFormat("Sending previous subscription information to {0}", ep);
                Client.Send(m, ep.Address.ToString(), ep.Port);
            }
            _log.Debug("Checking for relevant event correlations");

            if (handles != null)
            {
                /*
                 * Process event correlations based on the subscription handles from the subscribe process
                 */
                foreach (var handle in handles)
                {
                    /*
                     * 1. search for correlations of this handle
                     * 2. send a message for each correlation#
                     */
                    var correlations = EllaModel.Instance.GetEventCorrelations(handle.EventHandle);
                    foreach (var correlation in correlations)
                    {
                        KeyValuePair<EventHandle, EventHandle> pair =
                            new KeyValuePair<EventHandle, EventHandle>(handle.EventHandle, correlation);
                        Message m = new Message()
                            {
                                Type = MessageType.EventCorrelation,
                                Data = Serializer.Serialize(pair)
                            };
                        Client.SendAsync(m, ep.Address.ToString(), ep.Port);
                    }
                }
            }
        }

        internal static void ProcessUnsubscribe(MessageEventArgs e)
        {
            int ID = e.Message.Id;

            List<SubscriptionBase> remoteSubs =
                EllaModel.Instance.Subscriptions.FindAll(s => s.Handle is RemoteSubscriptionHandle);

            remoteSubs = remoteSubs.FindAll(s => (s.Handle as RemoteSubscriptionHandle).SubscriptionReference == ID);

            foreach (var sub in remoteSubs)
            {
                Ella.Unsubscribe.From(sub.Subscriber as Proxy, sub.Handle);
            }

        }
        #endregion
        #region Publish
        private static void ProcessPublish(MessageEventArgs e)
        {
            /*
                * Remote publisher is identified by
                * Remote node ID (contained in the message object)
                * Remote publisher ID
                * Remote publisher-event ID
                * The message reference (message ID used for the subscribe message)
                * Subscriber node ID
                * Assume shorts for all
                *   
            */
            short publisherID = BitConverter.ToInt16(e.Message.Data, 0);
            short eventID = BitConverter.ToInt16(e.Message.Data, 2);
            RemoteSubscriptionHandle handle = new RemoteSubscriptionHandle
            {
                EventID = eventID,
                PublisherId = publisherID,
                PublisherNodeID = e.Message.Sender,
                SubscriberNodeID = EllaConfiguration.Instance.NodeId
            };
            byte[] data = new byte[e.Message.Data.Length - 4];
            Buffer.BlockCopy(e.Message.Data, 4, data, 0, data.Length);
            var subscriptions = from s in EllaModel.Instance.Subscriptions
                                let h = (s.Handle as RemoteSubscriptionHandle)
                                where h != null && h == handle
                                select s;
            var subs = subscriptions as SubscriptionBase[] ?? subscriptions.ToArray();
            foreach (var sub in subs)
            {
                (sub.Event.Publisher as Stub).NewMessage(data);
            }
        }
        #endregion
        private static void ProcessApplicationMessageResponse(MessageEventArgs e)
        {
            ApplicationMessage msg = Serializer.Deserialize<ApplicationMessage>(e.Message.Data);
            object subscriber = (from s in EllaModel.Instance.Subscriptions
                                 where EllaModel.Instance.GetSubscriberId(s.Subscriber) == msg.Handle.SubscriberId
                                 select s.Subscriber).FirstOrDefault();
            if (subscriber != null)
                Send.DeliverMessage(msg, subscriber);
            else
            {
                _log.FatalFormat("No suitable subscriber for message reply found", msg);
            }
        }

        private static void ProcessEventCorrelation(MessageEventArgs e)
        {
            var correlation = Serializer.Deserialize<KeyValuePair<EventHandle, EventHandle>>(e.Message.Data);
            EventHandle first = correlation.Key;
            EventHandle second = correlation.Value;
            /*
             * Add to list of correlations?
             * Deliver to subscribers
             *      The subscriptions point directly to the subscriber instances, the handle is matching already
             */
            var results =
                EllaModel.Instance.Subscriptions.GroupBy(s => s.Subscriber)
                         .Where(
                             g =>
                             g.Any(g1 => Equals(g1.Handle.EventHandle, first)) &&
                             g.Any(g2 => Equals(g2.Handle.EventHandle, second)))
                         .Select(
                             g =>
                             new
                                 {
                                     Object = g.Key,
                                     Method =
                                 ReflectionUtils.GetAttributedMethod(g.Key.GetType(), typeof(AssociateAttribute))
                                 });
            foreach (var result in results)
            {
                if (result.Method != null)
                {
                    if (result.Method.GetParameters().Count() != 2 || result.Method.GetParameters().Any(p => p.ParameterType != typeof(SubscriptionHandle)))
                        throw new IllegalAttributeUsageException(String.Format("Method {0} attributed as Associate has invalid parameters (count or type)", result.Method));
                    int subscriberid = EllaModel.Instance.GetSubscriberId(result.Object);
                    RemoteSubscriptionHandle handle1 = new RemoteSubscriptionHandle() { EventHandle = first, SubscriberNodeID = EllaConfiguration.Instance.NodeId, SubscriberId = subscriberid };
                    RemoteSubscriptionHandle handle2 = new RemoteSubscriptionHandle() { EventHandle = second, SubscriberNodeID = EllaConfiguration.Instance.NodeId, SubscriberId = subscriberid };

                    result.Method.Invoke(result.Object, new object[] { handle1, handle2 });
                    result.Method.Invoke(result.Object, new object[] { handle2, handle1 });
                }


            }
        }

        private void ProcessNodeShutdown(MessageEventArgs e)
        {
            /*
             * Delete all subscriptions to this node
             * Delete all subscriptions from this node
             * Delete node from node dictionary
             * 
             * No communications to the node should be done, since it's shutdown already
             */


            //Subscriptions where local subscribers are subscribed to publishers from the node being shut down
            //in this case, the local stubs must be stopped and the subscriptions removed from the list
            SubscriptionController.PerformUnsubscribe(s => s.Handle.EventHandle.PublisherNodeId == e.Message.Sender, performRemoteUnsubscribe: false);
            //Subscriptions of modules on the remote node being subscribed to local publishers
            //Here, the proxy
            var subscriptionsFromRemoteNode = EllaModel.Instance.Subscriptions.Where(s => s.Handle is RemoteSubscriptionHandle).Where(s => (s.Handle as RemoteSubscriptionHandle).SubscriberNodeID == e.Message.Sender);
            foreach (var sub in subscriptionsFromRemoteNode)
            {
                Ella.Unsubscribe.From(sub.Subscriber as Proxy, sub.Handle);
            }

            _remoteHosts.Remove(e.Message.Sender);


        }
    }
}
