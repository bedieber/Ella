using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ella.Network;

namespace Ella
{
    public class FakeProxy : Proxy
    {
        internal bool eventReceived = false;
        public new void HandleEvent(object data)
        {
            if (data is String || data is DateTime)
                eventReceived = true;
        }


    }
}
