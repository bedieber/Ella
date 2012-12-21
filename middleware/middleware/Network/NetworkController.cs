using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Ella.Internal;
using Ella.Model;
using Ella.Network.Communication;
using log4net;

namespace Ella.Network
{
    internal class NetworkController
    {
        private static readonly NetworkController _instance = new NetworkController();

        private Server _server;
        private readonly Dictionary<int, EndPoint> _remoteHosts = new Dictionary<int, EndPoint>();

        private static ILog _log = LogManager.GetLogger(typeof(NetworkController));

        /// <summary>
        /// Starts the network controller.
        /// </summary>
        internal static void Start()
        {
            _instance._server = new Server(33333, IPAddress.Any);
            _instance._server.NewMessage += _instance.NewMessage;
            _instance._server.Start();
        }


        /// <summary>
        /// Subscribes to remote host.
        /// </summary>
        /// <typeparam name="T">The type to subscribe to</typeparam>
        internal static void SubscribeToRemoteHost<T>()
        {
            //TODO we're never creating a stub
            _instance.SubscribeTo(typeof(T));
        }

        internal static bool IsRunning { get { return _instance._server != null; } }
        /// <summary>
        /// Subscribes to a remote host.
        /// </summary>
        /// <param name="type">The type.</param>
        private void SubscribeTo(Type type)
        {
            //TODO Sender
            Message m = new Message { Type = MessageType.Subscribe, Data = Serializer.Serialize(type) };
            foreach (IPEndPoint address in _remoteHosts.Values)
            {
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
            _log.DebugFormat("New {1} message from {0}", e.Address, e.Message.Type);
            switch (e.Message.Type)
            {
                case MessageType.Discover:
                    {
                        if (!_remoteHosts.ContainsKey(e.Message.Id))
                        {
                            _log.InfoFormat("Discovered host {0}", e.Message.Id);
                            _remoteHosts.Add(e.Message.Sender, e.Address);
                        }
                        break;
                    }
                case MessageType.Publish:
                    {
                        /*
                         * Remote publisher is identified by
                         * Remote node ID (contained in the message object)
                         * Remote publisher ID
                         * Remote publisher-event ID
                         * Assume shorts for all
                         */
                        short publisherID = BitConverter.ToInt16(e.Message.Data, 0);
                        short eventID = BitConverter.ToInt16(e.Message.Data, 2);
                        RemoteSubscriptionHandle handle = new RemoteSubscriptionHandle
                            {
                                EventID = eventID,
                                PublisherID = publisherID,
                                RemoteNodeID = e.Message.Sender,
                            };
                        byte[] data = new byte[e.Message.Data.Length - 4];
                        Buffer.BlockCopy(e.Message.Data, 4, data, 0, data.Length);
                        var subscriptions = from s in EllaModel.Instance.Subscriptions let h = (s.Handle as RemoteSubscriptionHandle) where h != null && h == handle select s;
                        foreach (var sub in subscriptions)
                        {
                            (sub.Subscriber as Stub).NewMessage(data);
                        }

                        break;
                    }
                case MessageType.Subscribe:
                    {
                        Type type = Serializer.Deserialize<Type>(e.Message.Data);
                        Subscribe.RemoteSubscriber(type, e.Message.Sender);
                        break;
                    }
                case MessageType.SubscribeResponse:
                    {
                        var type = Serializer.Deserialize<Type>(e.Message.Data, 0);
                        var stubs = from s in EllaModel.Instance.Subscriptions
                                    where s.Event.Publisher is Stub && (s.Event.Publisher as Stub).DataType == type
                                    select s;
                        //TODO what now? Here would be the place for a template object
                        break;
                    }
            }

        }



    }
}
