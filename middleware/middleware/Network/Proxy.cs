using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Ella.Attributes;
using Ella.Data;
using Ella.Internal;
using Ella.Model;
using Ella.Network.Communication;

namespace Ella.Network
{
    /// <summary>
    /// A proxy is used to catch local events and transfer them to a remote subscriber stub
    /// </summary>
    [Subscriber()]
    internal class Proxy
    {
        internal Event EventToHandle { get; set; }
        internal IPEndPoint TargetNode { get; set; }

        [Factory]
        internal Proxy()
        {

        }

        /// <summary>
        /// Handles a new event by serializing and sending it to the remote subscriber
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="handle">The handle.</param>
        internal void HandleEvent(object data, SubscriptionHandle handle)
        {
          
            /*
             * check that incoming data object is serializable
             * Serialize it
             * Transfer
             */
            if (data.GetType().IsSerializable)
            {
                Message m = new Message();
                m.Type = MessageType.Publish;
                byte[] serialize = Serializer.Serialize(data);
                //PublisherID
                //EventID
                //data
                byte[] payload = new byte[serialize.Length + 4];
                Array.Copy(BitConverter.GetBytes((int)EllaModel.Instance.GetPublisherId(EventToHandle.Publisher)), payload, 2);
                Array.Copy(BitConverter.GetBytes(EventToHandle.EventDetail.ID), 0, payload, 2, 2);
                Array.Copy(serialize, 0, payload, 4, serialize.Length);
                m.Data = payload;
                Client.Send(m, TargetNode.Address.ToString(), TargetNode.Port);
            }

        }

       }
}
