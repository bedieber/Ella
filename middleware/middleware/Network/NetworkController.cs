using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Ella.Internal;
using Ella.Network.Communication;

namespace Ella.Network
{
    internal class NetworkController
    {
        private static readonly NetworkController _instance = new NetworkController();

        private Server _server;
        private Dictionary<int, EndPoint> _remoteHosts = new Dictionary<int, EndPoint>();

        /// <summary>
        /// Starts the network controller.
        /// </summary>
        internal static void Start()
        {
            _instance._server = new Server(33333, IPAddress.Any);
            _instance._server.Start();
        }


        /// <summary>
        /// Subscribes to remote host.
        /// </summary>
        /// <param name="type">The type.</param>
        internal static void SubscribeToRemoteHost(Type type)
        {
            _instance.SubscribeTo(type);
        }

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
            switch (e.Message.Type)
            {
                case MessageType.Discover:
                    {
                        if (!_remoteHosts.ContainsKey(e.Message.Id))
                            _remoteHosts.Add(e.Message.Id, e.Address);
                        break;
                    }
                case MessageType.Publish:
                    {
                        /*
                         * Remote publisher is identified by
                         * Remote node ID
                         * Remote publisher ID
                         * Remote publisher-event ID
                         * Assume shorts for all
                         */

                        short nodeID = BitConverter.ToInt16(e.Message.Data, 0);
                        short publisherID = BitConverter.ToInt16(e.Message.Data, 2);
                        short eventID = BitConverter.ToInt16(e.Message.Data, 4);
                        byte[] data = new byte[e.Message.Data.Length - 6];
                        Buffer.BlockCopy(e.Message.Data, 6, data, 0, data.Length);


                        break;
                    }
                    case MessageType.Subscribe:
                    {
                        Type type = Serializer.Deserialize<Type>(e.Message.Data);
                        Subscribe.RemoteSubscriber(type);
                        break;
                    }

            }

        }



    }
}
