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
        public static int countMsg ;
        public static bool unsubscribed = false;
        public static bool started = false;
        public static bool connectedToMulticastgroup=false;

        public FakeNetworkController()
        {
            Subscriptions = new Dictionary<Type, int>();
            countMsg = 0;
        }
        public void SubscribeTo(Type type, Action<RemoteSubscriptionHandle> callback)
        {

            if (Subscriptions.ContainsKey(type))
            {
                Subscriptions[type]++;
            }
            else
            {
                Subscriptions.Add(type, 1);
            }
        }

        public bool SendMessage(ApplicationMessage message, RemoteSubscriptionHandle remoteSubscriptionHandle, bool isReply = false)
        {
            countMsg++;

            return true;
        }

        public void UnsubscribeFrom(int subscriptionReference, int nodeId)
        {
            unsubscribed = true;
        }

        public void SendShutdownMessage()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            started = true;
        }

        public bool IsRunning { get { return true; } }

        public void ConnectToMulticastGroup(string @group, int port)
        {
            connectedToMulticastgroup = true;
        }
    }
}
