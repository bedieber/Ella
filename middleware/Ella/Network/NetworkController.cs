using System;
using System.Collections.Generic;
using System.Net;
using Ella.Control;
using Ella.Internal;
using Ella.Network.Communication;

namespace Ella.Network
{
    internal partial class NetworkController
    {

        private Server _server;
        private readonly Dictionary<int, EndPoint> _remoteHosts = new Dictionary<int, EndPoint>();


        private Dictionary<int, Action<RemoteSubscriptionHandle>> _pendingSubscriptions =
            new Dictionary<int, Action<RemoteSubscriptionHandle>>();
        private Dictionary<int, Type> _subscriptionCache = new Dictionary<int, Type>();

        /// <summary>
        /// Subscribes to a remote host.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="callback"></param>
        private void SubscribeTo(Type type, Action<RemoteSubscriptionHandle> callback)
        {
            Message m = new Message { Type = MessageType.Subscribe, Data = Serializer.Serialize(type) };
            //TODO when to remove?
            _pendingSubscriptions.Add(m.Id, callback);
            _subscriptionCache.Add(m.Id, type);
            foreach (IPEndPoint address in _remoteHosts.Values)
            {
                Client.SendAsync(m, address.Address.ToString(), address.Port);
            }
        }



        private bool SendMessage(ApplicationMessage message, RemoteSubscriptionHandle remoteSubscriptionHandle, bool isReply = false)
        {
            Message m = new Message { Data = Serializer.Serialize(message), Type = isReply ? MessageType.ApplicationMessageResponse : MessageType.ApplicationMessage };

            IPEndPoint ep = (IPEndPoint)_remoteHosts[isReply ? remoteSubscriptionHandle.SubscriberNodeID : remoteSubscriptionHandle.PublisherNodeID];
            if (ep != null)
            {
                Client.SendAsync(m, ep.Address.ToString(), ep.Port);
                return true;
            }
            return false;
        }



        private void UnsubscribeFrom(int subscriptionReference, int nodeId)
        {
            Message m = new Message(subscriptionReference) { Type = MessageType.Unsubscribe };
            var ipEndPoint = ((IPEndPoint)_remoteHosts[nodeId]);
            Client.SendAsync(m, ipEndPoint.Address.ToString(), ipEndPoint.Port);
        }

        private void SendShutdownMessage()
        {
            Message m = new Message { Type = MessageType.NodeShutdown };
            foreach (var host in _remoteHosts)
            {
                IPEndPoint address = (IPEndPoint)host.Value;
                Client.SendAsync(m, address.Address.ToString(), address.Port);
            }
        }


        /// <summary>
        /// Handles a new message from the network
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MessageEventArgs" /> instance containing the event data.</param>
        private void NewMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Sender == EllaConfiguration.Instance.NodeId)
                return;
            _log.DebugFormat("New {1} message from {0}", e.Address, e.Message.Type);
            switch (e.Message.Type)
            {
                case MessageType.Discover:
                    {
                        ProcessDiscover(e);
                        break;
                    }
                case MessageType.DiscoverResponse:
                    {
                        ProcessDiscoverResponse(e);
                        break;
                    }
                case MessageType.Publish:
                    {
                        ProcessPublish(e);

                        break;
                    }
                case MessageType.Subscribe:
                    {
                        ProcessSubscribe(e);
                        break;
                    }
                case MessageType.SubscribeResponse:
                    {
                        ProcessSubscribeResponse(e);
                        break;
                    }
                case MessageType.Unsubscribe:
                    {
                        ProcessUnsubscribe(e);
                        break;
                    }
                case MessageType.ApplicationMessage:
                    {
                        ApplicationMessage msg = Serializer.Deserialize<ApplicationMessage>(e.Message.Data);
                        Send.DeliverApplicationMessage(msg);
                        break;
                    }
                case MessageType.ApplicationMessageResponse:
                    {
                        ProcessApplicationMessageResponse(e);
                        break;
                    }
                case MessageType.EventCorrelation:
                    ProcessEventCorrelation(e);
                    break;
                case MessageType.NodeShutdown:
                    ProcessNodeShutdown(e);
                    break;

            }
        }


    }
}
