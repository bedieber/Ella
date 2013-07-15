//=============================================================================
// Project  : Ella Middleware
// File    : NetworkController.cs
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
using System.Net;
using Ella.Control;
using Ella.Internal;
using Ella.Network;
using Ella.Network.Communication;
using log4net;

namespace Ella.Controller
{
    /// <summary>
    /// A network controller for IP-based networks
    /// </summary>
    internal class NetworkController : INetworkController
    {
        private ILog _log = LogManager.GetLogger(typeof(NetworkController));
        private readonly Dictionary<int, EndPoint> _remoteHosts = new Dictionary<int, EndPoint>();
        private readonly MessageProcessor _messageProcessor = new MessageProcessor();

        public List<INetworkServer> Servers { get; private set; }


        public NetworkController()
        {
            Servers = new List<INetworkServer>();
        }

        /// <summary>
        /// Subscribes to a remote host.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="callback"></param>
        public void SubscribeTo(Type type, Action<RemoteSubscriptionHandle> callback)
        {
            Message m = new Message { Type = MessageType.Subscribe, Data = Serializer.Serialize(type) };
            //TODO when to remove?
            _messageProcessor.PendingSubscriptions.Add(m.Id, callback);
            _messageProcessor.SubscriptionCache.Add(m.Id, type);
            foreach (IPEndPoint ep in _remoteHosts.Values)
            {
                SenderBase.CreateSender(ep).SendAsync(m);
            }
        }


        public bool SendMessage(ApplicationMessage message, RemoteSubscriptionHandle remoteSubscriptionHandle, bool isReply = false)
        {
            Message m = new Message { Data = Serializer.Serialize(message), Type = isReply ? MessageType.ApplicationMessageResponse : MessageType.ApplicationMessage };

            IPEndPoint ep = (IPEndPoint)_remoteHosts[isReply ? remoteSubscriptionHandle.SubscriberNodeID : remoteSubscriptionHandle.PublisherNodeID];
            if (ep != null)
            {
                SenderBase.SendAsync(m, ep);
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
            var ep = ((IPEndPoint)_remoteHosts[nodeId]);
            SenderBase.SendAsync(m, ep);
        }

        /// <summary>
        /// Sends a shutdown message.
        /// </summary>
        public void SendShutdownMessage()
        {
            Message m = new Message { Type = MessageType.NodeShutdown };
            foreach (var host in _remoteHosts)
            {
                var ep = host.Value;
                SenderBase.SendAsync(m, ep);
            }
        }
        /// <summary>
        /// Starts the networkController
        /// </summary>
        public void Start()
        {
            Servers.ForEach(delegate(INetworkServer s)
            {
                s.NewMessage += _messageProcessor.NewMessage;
                s.Start(); });
        }

        /// <summary>
        /// Indicates if the controller is already running
        /// </summary>
        public bool IsRunning { get { return Servers.Any(); } }


        /// <summary>
        /// Connects all multicast-enabled servers to the specified group
        /// </summary>
        /// <param name="group">Multicast group</param>
        /// <param name="port">Multicast group port</param>
        public void ConnectToMulticastGroup(string group, int port)
        {
            Servers.OfType<IMulticastListener>().ToList().ForEach(s => s.ConnectToMulticastGroup(group, port));
        }




    }
}
