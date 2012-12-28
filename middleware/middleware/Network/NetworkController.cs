using System;
using System.Collections;
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

        private Dictionary<int, Action<RemoteSubscriptionHandle>> _pendingSubscriptions =
            new Dictionary<int, Action<RemoteSubscriptionHandle>>();

        /// <summary>
        /// Starts the network controller.
        /// </summary>
        internal static void Start()
        {
            _instance._server = new Server(Properties.Ella.Default.NetworkPort, IPAddress.Any);
            _instance._server.NewMessage += _instance.NewMessage;
            _instance._server.Start();
        }


        /// <summary>
        /// Subscribes to remote host.
        /// </summary>
        /// <typeparam name="T">The type to subscribe to</typeparam>
        internal static void SubscribeToRemoteHost<T>(Action<RemoteSubscriptionHandle> callback)
        {
            _instance.SubscribeTo(typeof(T), callback);
        }

        internal static bool IsRunning { get { return _instance._server != null; } }

        /// <summary>
        /// Subscribes to a remote host.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="callback"></param>
        private void SubscribeTo(Type type, Action<RemoteSubscriptionHandle> callback)
        {
            Message m = new Message { Type = MessageType.Subscribe, Data = Serializer.Serialize(type)};
            //TODO when to remove?
            _pendingSubscriptions.Add(m.Id, callback);
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
                        IEnumerable<RemoteSubscriptionHandle> handles = Subscribe.RemoteSubscriber(type, e.Message.Sender);
                        if (handles != null)
                        {
                            //Send reply
                            byte[] handledata = Serializer.Serialize(handles);
                            byte[] reply = new byte[handledata.Length + 4];
                            byte[] idbytes = BitConverter.GetBytes(e.Message.Id);
                            Array.Copy(idbytes, reply, idbytes.Length);
                            Array.Copy(handledata, 0, reply, idbytes.Length, handledata.Length);
                            Message m = new Message { Type = MessageType.SubscribeResponse, Data = reply };
                            //TODO check port
                            Client.Send(m, e.Address.ToString(), (e.Address as IPEndPoint).Port);
                        }
                        break;
                    }
                case MessageType.SubscribeResponse:
                    {
                        if (e.Message.Data.Length > 0)
                        {
                            int inResponseTo = BitConverter.ToInt32(e.Message.Data, 0);
                            ICollection<RemoteSubscriptionHandle> handles = Serializer.Deserialize<ICollection<RemoteSubscriptionHandle>>(e.Message.Data, 4);
                            //var stubs = from s in EllaModel.Instance.Subscriptions
                            //            where s.Event.Publisher is Stub && (s.Event.Publisher as Stub).DataType == type
                            //            select s;

                            if (_pendingSubscriptions.ContainsKey(inResponseTo))
                            {
                                Action<RemoteSubscriptionHandle> action = _pendingSubscriptions[inResponseTo];
                                foreach (var handle in handles)
                                {
                                    action(handle);
                                }
                            }
                        }
                        break;
                    }
            }

        }



    }
}
