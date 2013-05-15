using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Ella.Internal;
using Ella.Network.Communication;

namespace Ella.Network
{
    internal class MulticastProxy : Proxy
    {
        private int _multicastPort;
        internal static int NextFreeMulticastPort;

        static MulticastProxy()
        {
            NextFreeMulticastPort = EllaConfiguration.Instance.DiscoveryPortRangeEnd + (EllaConfiguration.Instance.NodeId - 1) *
                                    EllaConfiguration.Instance.MulticastPortRangeSize;
        }

        internal MulticastProxy()
        {
            _multicastPort = Interlocked.Increment(ref MulticastProxy.NextFreeMulticastPort);
            TargetNode.Address = EllaConfiguration.Instance.MulticastAdress;
            TargetNode.Port = _multicastPort;
        }

        protected override void Send(Message m)
        {
            Client.SendUdp(m, TargetNode.Address.ToString(), TargetNode.Port);
        }
    }
}
