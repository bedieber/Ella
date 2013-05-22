//=============================================================================
// Project  : Ella Middleware
// File    : MulticastProxy.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@uni-klu.ac.at)
// Copyright 2012 by Bernhard Dieber, Jennifer Simonjan
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
using System.Text;
using System.Threading;
using Ella.Internal;
using Ella.Network.Communication;
using log4net;

namespace Ella.Network
{
    internal class MulticastProxy : Proxy
    {
        private static ILog _log = LogManager.GetLogger(typeof (MulticastProxy));
        private int _multicastPort;
        internal static int NextFreeMulticastPort;

        static MulticastProxy()
        {
            NextFreeMulticastPort = EllaConfiguration.Instance.DiscoveryPortRangeEnd + (EllaConfiguration.Instance.NodeId - 1) *
                                    EllaConfiguration.Instance.MulticastPortRangeSize;
            _log.DebugFormat("Next free multicast port is {0}.",NextFreeMulticastPort);
        }

        internal MulticastProxy()
        {
            _multicastPort = Interlocked.Increment(ref MulticastProxy.NextFreeMulticastPort);
            _log.DebugFormat("Used multicast port is {0}",_multicastPort);
            TargetNode = new IPEndPoint(IPAddress.Parse(EllaConfiguration.Instance.MulticastAddress), _multicastPort);
            _log.DebugFormat("Target node with IPAddress {0} and port {1}",TargetNode.Address,TargetNode.Port);
        }

        protected override void Send(Message m)
        {
            _log.DebugFormat("Try to send over UDP, Message length is {0}",m.Data.Length);
            Client.SendUdp(m, TargetNode.Address.ToString(), TargetNode.Port);
        }
    }
}
