using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Ella.Internal;
using log4net;

namespace Ella.Network.Communication
{

    /// <summary>
    /// The networking client used to contact a remote endpoint
    /// </summary>
    internal class Client
    {
        private static ILog _log = LogManager.GetLogger(typeof(Client));

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        internal static void Send(Message m, string address, int port)
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(IPAddress.Parse(address), port);
                NetworkStream stream = client.GetStream();
                byte[] serialize = m.Serialize();
                stream.Write(serialize, 0, serialize.Length);
                stream.Flush();
                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                _log.WarnFormat("NetworkClient: failed to send message {0} to {1}", m.Id,
                                  address, e.Message);
            }
        }

        /// <summary>
        /// Sends the specified Message <paramref name="m"/> asynchronously. This call returns immediately.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="sendingFinishedCallback">The sending finished callback.</param>
        internal static void SendAsync(Message m, string address, int port, Action<int> sendingFinishedCallback = null)
        {
            new Thread((ThreadStart)delegate
            {
                Send(m, address, port);
                if (sendingFinishedCallback != null)
                {
                    sendingFinishedCallback(m.Id);
                }
            }).Start();
        }

        internal static void Broadcast()
        {
            UdpClient client = new UdpClient();
            byte[] idBytes = BitConverter.GetBytes(EllaConfiguration.Instance.NodeId);
            byte[] portBytes = BitConverter.GetBytes(EllaConfiguration.Instance.NetworkPort);
            byte[] bytes = new byte[idBytes.Length + portBytes.Length];
            Array.Copy(idBytes, bytes, idBytes.Length);
            Array.Copy(portBytes, 0, bytes, idBytes.Length, portBytes.Length);
            //TODO handle invalid port range
            for (int i = EllaConfiguration.Instance.DiscoveryPortRangeStart;
                 i <= EllaConfiguration.Instance.DiscoveryPortRangeEnd;
                 i++)
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Parse("255.255.255.255"), i);
                client.Send(bytes, bytes.Length, ip);
            }
        }
    }
}