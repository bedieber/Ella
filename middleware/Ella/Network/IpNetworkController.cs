//=============================================================================
// Project  : Ella Middleware
// File    : IpNetworkController.cs
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
using System.Net;
using Ella.Control;
using Ella.Internal;
using Ella.Network.Communication;
using log4net;

namespace Ella.Network
{
    /// <summary>
    /// A network controller for IP-based networks
    /// </summary>
    internal partial class IpNetworkController : INetworkController
    {
        private ILog _log = LogManager.GetLogger(typeof (IpNetworkController));

        private Server _server;
        private UdpServer _udpServer;
        private readonly Dictionary<int, EndPoint> _remoteHosts = new Dictionary<int, EndPoint>();


        private Dictionary<int, Action<RemoteSubscriptionHandle>> _pendingSubscriptions =
            new Dictionary<int, Action<RemoteSubscriptionHandle>>();
        private Dictionary<int, Type> _subscriptionCache = new Dictionary<int, Type>();

        /// <summary>
        /// Subscribes to a remote host.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="callback"></param>
        public void SubscribeTo(Type type, Action<RemoteSubscriptionHandle> callback)
        {
            Message m = new Message { Type = MessageType.Subscribe, Data = Serializer.Serialize(type) };
            //TODO when to remove?
            _pendingSubscriptions.Add(m.Id, callback);
            _subscriptionCache.Add(m.Id, type);
            foreach (IPEndPoint address in _remoteHosts.Values)
            {
                Sender.SendAsync(m, address.Address.ToString(), address.Port);
            }
        }


        public bool SendMessage(ApplicationMessage message, RemoteSubscriptionHandle remoteSubscriptionHandle, bool isReply = false)
        {
            Message m = new Message { Data = Serializer.Serialize(message), Type = isReply ? MessageType.ApplicationMessageResponse : MessageType.ApplicationMessage };

            IPEndPoint ep = (IPEndPoint)_remoteHosts[isReply ? remoteSubscriptionHandle.SubscriberNodeID : remoteSubscriptionHandle.PublisherNodeID];
            if (ep != null)
            {
                Sender.SendAsync(m, ep.Address.ToString(), ep.Port);
                return true;
            }
            return false;
        }



        /// <summary>
        /// Unsubscribes a node from the subscription defined by the subscription reference.
        /// </summary>
        /// <param name="subscriptionReference">The subscription reference.</param>
        /// <param name="nodeId">The node id.</param>
        public void UnsubscribeFrom(int subscriptionReference, int nodeId)
        {
            Message m = new Message(subscriptionReference) { Type = MessageType.Unsubscribe };
            var ipEndPoint = ((IPEndPoint)_remoteHosts[nodeId]);
            Sender.SendAsync(m, ipEndPoint.Address.ToString(), ipEndPoint.Port);
        }

        /// <summary>
        /// Sends a shutdown message.
        /// </summary>
        public void SendShutdownMessage()
        {
            Message m = new Message { Type = MessageType.NodeShutdown };
            foreach (var host in _remoteHosts)
            {
                IPEndPoint address = (IPEndPoint)host.Value;
                Sender.SendAsync(m, address.Address.ToString(), address.Port);
            }
        }
        /// <summary>
        /// Starts the networkController
        /// </summary>
        public void Start()
        {
            _udpServer = new UdpServer(EllaConfiguration.Instance.NetworkPort);
            _udpServer.NewMessage += NewMessage;
            _udpServer.Start();
            _server = new Server(EllaConfiguration.Instance.NetworkPort, IPAddress.Any);
            _server.NewMessage += NewMessage;
            _server.Start();
        }

        /// <summary>
        /// Indicates if the controller is already running
        /// </summary>
        public bool IsRunning { get { return _server != null; }}

        public void ConnectToMulticastGroup(string group, int port)
        {
            _udpServer.ConnectToMulticastGroup(group, port);
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
