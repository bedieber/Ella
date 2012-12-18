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
        internal new void HandleEvent(object data)
        {
            if (data is String)
                eventReceived = true;
        }
    }
}
