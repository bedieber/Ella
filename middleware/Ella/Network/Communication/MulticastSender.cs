//=============================================================================
// Project  : Ella Middleware
// File    : MulticastSender.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System.Net;
using System.Threading;
using Ella.Internal;
using log4net;

namespace Ella.Network.Communication
{
    /// <summary>
    /// The networking client used to contact a remote endpoint using UDP
    /// </summary>
    internal class MulticastSender
    {
        private static ILog _log = LogManager.GetLogger(typeof (MulticastSender));
        private int _multicastPort;
        internal static int NextFreeMulticastPort;
        internal IPEndPoint TargetNode;

        /// <summary>
        /// Initializes the <see cref="MulticastSender"/> class.
        /// </summary>
        static MulticastSender()
        {
            NextFreeMulticastPort = EllaConfiguration.Instance.DiscoveryPortRangeEnd + (EllaConfiguration.Instance.NodeId - 1) *
                                    EllaConfiguration.Instance.MulticastPortRangeSize;
            _log.DebugFormat("Next free multicast port is {0}.",NextFreeMulticastPort);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MulticastSender"/> class.
        /// </summary>
        internal MulticastSender()
        {
            _multicastPort = Interlocked.Increment(ref MulticastSender.NextFreeMulticastPort);
            _log.DebugFormat("Used multicast port is {0}",_multicastPort);
            TargetNode = new IPEndPoint(IPAddress.Parse(EllaConfiguration.Instance.MulticastAddress), _multicastPort);
            _log.DebugFormat("Target node with IPAddress {0} and port {1}",TargetNode.Address,TargetNode.Port);
        }

        /// <summary>
        /// Sends the specified message over udp.
        /// </summary>
        /// <param name="m">The message.</param>
        internal void Send(Message m)
        {
            _log.DebugFormat("Try to send over UDP, Message length is {0}",m.Data.Length);
            Sender.SendUdp(m, TargetNode.Address.ToString(), TargetNode.Port);
        }
    }
}
