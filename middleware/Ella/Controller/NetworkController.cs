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
        private readonly MessageProcessor _messageProcessor = new MessageProcessor();

        public List<INetworkServer> Servers { get; private set; }


        public NetworkController(IEnumerable<KeyValuePair<int, string>> initialHostList)
        {
            Servers = new List<INetworkServer>();
            if (initialHostList != null)
            {
                foreach (var keyValuePair in initialHostList)
                {
                    if (keyValuePair.Key <= 0)
                    {
                        _log.WarnFormat("Could not add {0} to host list. Id is not within the valid range", keyValuePair.Key);
                        
                    }
                    var strings = keyValuePair.Value.Split(':');
                    IPAddress address = null;
                    if (!IPAddress.TryParse(strings[0], out address))
                    {
                        _log.WarnFormat("Could not add {0} to host list. Value has the wrong format", keyValuePair.Value);
                        continue;
                    }
                    int port;
                    if (!int.TryParse(strings[2], out port))
                    {
                        _log.WarnFormat("Could not add {0} to host list. Value has the wrong format", keyValuePair.Value);
                        continue;
                    }
                    if (_messageProcessor.RemoteHosts.ContainsKey(keyValuePair.Key))
                    {
                        _log.WarnFormat("Could not add node {0} to host list. A host with the same id already exists", keyValuePair.Key);
                        continue;
                    }

                    _messageProcessor.RemoteHosts.Add(keyValuePair.Key, new IPEndPoint(address,port));
                }
            }
        }

        /// <summary>
        /// Subscribes to a remote host.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="callback"></param>
        public void SubscribeTo(Type type, Action<RemoteSubscriptionHandle> callback)
        {
            Message m = new Message { Type = MessageType.Subscribe, Data = Serializer.Serialize(type) };
            lock (_messageProcessor.PendingSubscriptions)
            {
                _messageProcessor.PendingSubscriptions.Add(m.Id, callback);
            }
            lock (_messageProcessor.SubscriptionCache)
            {
                _messageProcessor.SubscriptionCache.Add(m.Id, type);
            }
            foreach (IPEndPoint ep in _messageProcessor.RemoteHosts.Values.ToArray())
            {
                SenderBase.CreateSender(ep).SendAsync(m);
            }
        }


        public bool SendMessage(ApplicationMessage message, RemoteSubscriptionHandle remoteSubscriptionHandle, bool isReply = false)
        {
            Message m = new Message { Data = Serializer.Serialize(message), Type = isReply ? MessageType.ApplicationMessageResponse : MessageType.ApplicationMessage };

            IPEndPoint ep = (IPEndPoint)_messageProcessor.RemoteHosts[remoteSubscriptionHandle.PublisherNodeID == EllaConfiguration.Instance.NodeId ? remoteSubscriptionHandle.SubscriberNodeID : remoteSubscriptionHandle.PublisherNodeID];
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
            var ep = ((IPEndPoint)_messageProcessor.RemoteHosts[nodeId]);
            SenderBase.SendAsync(m, ep);
        }

        /// <summary>
        /// Sends a shutdown message.
        /// </summary>
        public void SendShutdownMessage()
        {
            Message m = new Message { Type = MessageType.NodeShutdown };
            foreach (var host in _messageProcessor.RemoteHosts)
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
                s.Start();
            });
        }

        /// <summary>
        /// Stops the network controller and all servers
        /// </summary>
        public void Stop()
        {
            Servers.ForEach(s => s.Stop());
        }

        public void BroadcastMessage(Message msg)
        {
            var remoteHosts = _messageProcessor.RemoteHosts.ToArray();
            foreach (var remoteHost in remoteHosts)
            {
                SenderBase.SendAsync(msg, remoteHost.Value);
            }
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
