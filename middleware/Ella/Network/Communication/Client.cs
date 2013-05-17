using System;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
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
                IAsyncResult ar = client.BeginConnect(IPAddress.Parse(address), port, null, null);

                if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false))
                {
                    client.Close();
                    _log.WarnFormat("Could not connect to {0} in time, aborting send operation", address);
                    return;
                }

                client.EndConnect(ar);
                GZipStream stream = new GZipStream(client.GetStream(), CompressionMode.Compress);
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

        internal static void SendUdp(Message msg, string address, int port)
        {
            UdpClient client = new UdpClient();
            client.Connect(IPAddress.Parse(address), port);

            byte[] b = msg.Serialize();
            client.Send(b, b.Length);

            client.Close();

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

        /// <summary>
        /// Broadcasts this instance.
        /// </summary>
        internal static void Broadcast()
        {
            byte[] idBytes = BitConverter.GetBytes(EllaConfiguration.Instance.NodeId);
            byte[] portBytes = BitConverter.GetBytes(EllaConfiguration.Instance.NetworkPort);
            byte[] bytes = new byte[idBytes.Length + portBytes.Length];
            Array.Copy(idBytes, bytes, idBytes.Length);
            Array.Copy(portBytes, 0, bytes, idBytes.Length, portBytes.Length);

            /*
             * Iterate over all port numbers in the configuration port range
             */
            for (int i = EllaConfiguration.Instance.DiscoveryPortRangeStart;
                 i <= EllaConfiguration.Instance.DiscoveryPortRangeEnd;
                 i++)
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Parse("255.255.255.255"), i);

                /*
                 * Iterate over all network interfaces to perform discovery on each IF
                 */
                NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface nIf in allNetworkInterfaces)
                {
                    foreach (UnicastIPAddressInformation ua in nIf.GetIPProperties().UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily != AddressFamily.InterNetwork || IPAddress.IsLoopback(ua.Address))
                            continue;
                        try
                        {
                            UdpClient client = new UdpClient(new IPEndPoint(ua.Address, 0));
                            client.Send(bytes, bytes.Length, ip);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }
    }
}