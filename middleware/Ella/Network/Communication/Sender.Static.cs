//=============================================================================
// Project  : Ella Middleware
// File    : Sender.cs
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
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Ella.Internal;
using log4net;

namespace Ella.Network.Communication
{
    internal partial class Sender
    {
        private static ILog _log = LogManager.GetLogger(typeof(Sender));
        private static int _maxArraySize = 512;
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
                //NetworkStream stream = client.GetStream();
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
        /// Sends the specified message over UDP.
        /// </summary>
        /// <param name="msg">The message to be sent.</param>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        internal static void SendUdp(Message msg, string address, int port)
        {
            Socket udps = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udps.SendBufferSize = msg.Data.Length;
            udps.Connect(new IPEndPoint(IPAddress.Parse(address), port));

            byte[] b = msg.Serialize();

            udps.Send(b);
            udps.Close();
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
