using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Ella.Internal;
using Ella.Network;
using Ella.Network.Communication;

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

        public void SubscribeResponseMessageEvent(int msgId)
        {
            RemoteSubscriptionHandle h = new RemoteSubscriptionHandle();
            //SubscriptionReference has to be equal the msgID of the SubscribeMsg
            h.SubscriptionReference = msgId;
            h.PublisherId = 234;
            h.PublisherNodeID = EllaConfiguration.Instance.NodeId + 1;

            List<RemoteSubscriptionHandle> handles = new List<RemoteSubscriptionHandle>();
            handles.Add(h);

            byte[] handledata = Serializer.Serialize(handles);
            byte[] reply = new byte[handledata.Length + 4];
            byte[] idbytes = BitConverter.GetBytes(h.SubscriptionReference);
            
            Array.Copy(idbytes, reply, idbytes.Length);
            Array.Copy(handledata, 0, reply, idbytes.Length, handledata.Length);
            Message m = new Message { Type = MessageType.SubscribeResponse, Data = reply, Sender = h.PublisherNodeID};

            NewMessage(this, new MessageEventArgs(m));
        }
    }
}
