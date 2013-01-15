using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ella.Network;

namespace Ella
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
