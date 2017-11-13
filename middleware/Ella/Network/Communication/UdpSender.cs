//=============================================================================
// Project  : Ella Middleware
// File    : UdpSender.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: https://github.com/bedieber/Ella.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using Ella.Internal;
using Ella.Model;
using log4net;

namespace Ella.Network.Communication
{
    /// <summary>
    /// The networking client used to contact a remote endpoint using UDP
    /// </summary>
    internal class UdpSender : SenderBase
    {
        private static ILog _log = LogManager.GetLogger(typeof(UdpSender));
        internal static int NextFreeMulticastPort;
        internal IPEndPoint TargetNode;
        private static Timer _discoveryTimer;

        /// <summary>
        /// Initializes the <see cref="UdpSender"/> class.
        /// </summary>
        static UdpSender()
        {
            NextFreeMulticastPort = EllaConfiguration.Instance.DiscoveryPortRangeEnd + (EllaConfiguration.Instance.NodeId - 1) *
                                    EllaConfiguration.Instance.MulticastPortRangeSize;
            _log.DebugFormat("Next free multicast port is {0}.", NextFreeMulticastPort);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpSender"/> class.
        /// </summary>
        internal UdpSender()
        {
            int multicastPort = Interlocked.Increment(ref NextFreeMulticastPort);
            _log.DebugFormat("Used multicast port is {0}", multicastPort);
            TargetNode = new IPEndPoint(IPAddress.Parse(EllaConfiguration.Instance.MulticastAddress), multicastPort);
            _log.DebugFormat("Target node with IPAddress {0} and port {1}", TargetNode.Address, TargetNode.Port);
        }

        /// <summary>
        /// Sends the specified message over udp.
        /// </summary>
        /// <param name="m">The message.</param>
        internal override void Send(Message m)
        {
            _log.DebugFormat("Try to send over UDP, Message length is {0}", m.Data.Length);
            string address = TargetNode.Address.ToString();
            int port = TargetNode.Port;
            Socket udps = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udps.SendBufferSize = m.Data.Length;
            udps.Connect(new IPEndPoint(IPAddress.Parse(address), port));

            byte[] b = m.Serialize();

            udps.Send(b);
            udps.Close();
        }

        /// <summary>
        /// Broadcasts
        /// </summary>
        internal static void Broadcast()
        {
            TimerCallback broadcast = o =>
            {
                //TODO put sequence number in discovery requests
                //on processing check if sequence number is lower than the last known....then a restart has happened

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
                            if (!string.IsNullOrEmpty(EllaConfiguration.Instance.BindAddress) &&
                                EllaConfiguration.Instance.BindAddress != "0.0.0.0" &&
                                ua.Address.ToString() != EllaConfiguration.Instance.BindAddress)
                            {
                                continue;
                            }
                            if (ua.Address.AddressFamily != AddressFamily.InterNetwork
                                /*|| IPAddress.IsLoopback(ua.Address)*/)
                                //do not exclude local loopback, might be a multi-process single-node application
                                continue;
                            try
                            {
                                _log.DebugFormat("Broadcasting to NIC {0} port {1}", ua.Address, i);
                                UdpClient client = new UdpClient(new IPEndPoint(ua.Address, 0));
                                client.Send(bytes, bytes.Length, ip);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            };
            _discoveryTimer = new Timer(broadcast, null, 0, 5000);
        }

        public override void Dispose()
        {

        }
    }
}
