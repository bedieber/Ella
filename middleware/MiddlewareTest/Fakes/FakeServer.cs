using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Ella.Network;

namespace Ella.Fakes
{
    class FakeServer : INetworkServer 
    {
        public event MessageEventHandler NewMessage;
        
        public void Start()
        {
            
        }

        public void Stop()
        {
            
        }

        public void DiscoveryMessageEvent(Message msg, IPEndPoint ep)
        {
            NewMessage(this, new MessageEventArgs(msg) { Address = ep });
        }
    }
}
