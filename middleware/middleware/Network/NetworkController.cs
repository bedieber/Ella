using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Ella.Network.Communication;

namespace Ella.Network
{
    internal class NetworkController
    {
        private Server _server;
        private static readonly NetworkController _instance = new NetworkController();
        public static void Start()
        {
            _instance._server = new Server(33333, IPAddress.Any);
            _instance._server.Start();
        }

        private void NewMessage(object sender, MessageEventArgs e)
        {
            switch (e.Message.Type)
            {
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
            }

        }
    }
}
