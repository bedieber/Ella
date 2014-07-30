using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Ella.Network;
using Ella.Network.Communication;

namespace Ella.Fakes
{
    class FakeSender : SenderBase
    {
        internal readonly Dictionary<MessageType, int> _messages = new Dictionary<MessageType, int>();
        public int _id = 0;

        internal override void Send(Message m)
        {
            _id = m.Id;

            if (!_messages.ContainsKey(m.Type))
            {
                _messages.Add(m.Type, 1);
            }
            else
            {
                _messages[m.Type] += 1;
            }
        }

        public override void Dispose()
        {
            
        }
    }
}
