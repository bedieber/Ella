using System;
using Ella.Network;

namespace Ella.Fakes
{
    internal class FakeProxy : Proxy
    {
        internal bool eventReceived = false;
        internal new void HandleEvent(object data, SubscriptionHandle handle)
        {
            if (data is String || data is DateTime)
                eventReceived = true;
        }


    }
}
