using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Ella.Attributes;
using Ella.Data;
using Ella.Model;
using Ella.Network.Communication;

namespace Ella.Network
{
    /// <summary>
    /// A proxy is used to catch local events and transfer them to a remote subscriber stub
    /// </summary>
    [Subscriber(ModifyPolicy = DataModifyPolicy.NoModify)]
    internal class Proxy
    {
        internal Event EventToHandle { get; set; }
        internal IPEndPoint TargetNode { get; set; }
        private BinaryFormatter _formatter = new BinaryFormatter();

        [Factory]
        internal Proxy()
        {

        }

        internal void HandleEvent(object data)
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
                //TODO message sender
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {

                    try
                    {
                        bf.Serialize(ms, data);
                    }
                    catch (Exception)
                    {
                        //TODO log
                        return;
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                    m.Data = ms.ToArray();

                    Client.Send(m, TargetNode.Address.ToString(), TargetNode.Port);
                }
            }

        }
    }
}
