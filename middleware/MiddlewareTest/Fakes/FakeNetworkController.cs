using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ella.Control;
using Ella.Network;

namespace Ella.Fakes
{
    internal class FakeNetworkController : INetworkController
    {
        public static Dictionary<Type, int> Subscriptions { get; set; }

        public void SubscribeTo(Type type, Action<RemoteSubscriptionHandle> callback)
        {
            throw new NotImplementedException();
        }

        public bool SendMessage(ApplicationMessage message, RemoteSubscriptionHandle remoteSubscriptionHandle, bool isReply = false)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeFrom(int subscriptionReference, int nodeId)
        {
            throw new NotImplementedException();
        }

        public void SendShutdownMessage()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public bool IsRunning { get { return true; } }

        public void ConnectToMulticastGroup(string @group, int port)
        {
            throw new NotImplementedException();
        }
    }
}
