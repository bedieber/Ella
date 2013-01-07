using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ella.Internal;
using Ella.Model;
using Ella.Network.Communication;

namespace Ella.Network
{
    internal partial class NetworkController
    {
        #region Discovery
        private void ProcessDiscover(MessageEventArgs e)
        {
            /*
             * Respond to a discovery message
             */
            int port = BitConverter.ToInt32(e.Message.Data, 4);
            IPEndPoint ep = (IPEndPoint)e.Address;
            ep.Port = port;
            if (!_remoteHosts.ContainsKey(e.Message.Sender))
            {
                _log.InfoFormat("Discovered host {0} on {1}", e.Message.Sender, ep);
                _remoteHosts.Add(e.Message.Sender, ep);
            }
            byte[] portBytes = BitConverter.GetBytes(EllaConfiguration.Instance.NetworkPort);
            //byte[] bytes = new byte[idBytes.Length + portBytes.Length];
            //Array.Copy(idBytes, bytes, idBytes.Length);
            //Array.Copy(portBytes, 0, bytes, idBytes.Length, portBytes.Length);
            Message m = new Message { Type = MessageType.DiscoverResponse, Data = portBytes };
            Client.SendAsync(m, ep.Address.ToString(), ep.Port);
        }

        private void ProcessDiscoverResponse(MessageEventArgs e)
        {
            if (!_remoteHosts.ContainsKey(e.Message.Sender))
            {
                int port = BitConverter.ToInt32(e.Message.Data, 0);
                IPEndPoint ep = (IPEndPoint)e.Address;
                ep.Port = port;
                _log.InfoFormat("Host {0} on {1} responded to discovery", e.Message.Sender, ep);
                _remoteHosts.Add(e.Message.Sender, ep);
            }
        }
        #endregion
        #region Subscribe

        private void ProcessSubscribeResponse(MessageEventArgs e)
        {
            if (e.Message.Data.Length > 0)
            {
                int inResponseTo = BitConverter.ToInt32(e.Message.Data, 0);
                ICollection<RemoteSubscriptionHandle> handles =
                    Serializer.Deserialize<ICollection<RemoteSubscriptionHandle>>(e.Message.Data, 4);
                //var stubs = from s in EllaModel.Instance.Subscriptions
                //            where s.Event.Publisher is Stub && (s.Event.Publisher as Stub).DataType == type
                //            select s;

                if (_pendingSubscriptions.ContainsKey(inResponseTo))
                {
                    Action<RemoteSubscriptionHandle> action = _pendingSubscriptions[inResponseTo];
                    foreach (var handle in handles)
                    {
                        //TODO for some reason the remote node ID is not serialized, find out why.
                        handle.RemoteNodeID = e.Message.Sender;
                        action(handle);
                    }
                }
                else
                {
                    //TODO perform unsubscribe, reference may be relict from previous run
                    _log.WarnFormat(
                        "Detected invalid subscription reference {0}, may be from previous application run. Unsubscribing...",
                        inResponseTo);
                }
            }
        }

        private void ProcessSubscribe(MessageEventArgs e)
        {
            Type type = Serializer.Deserialize<Type>(e.Message.Data);
            //TODO handle case when remote host is not in remoteHosts dictionary
            //get the that this node is already subscribed for, to avoid double subscriptions
            var currentHandles = (from s in EllaModel.Instance.Subscriptions
                                  let s1 = (s.Handle as RemoteSubscriptionHandle)
                                  where
                                      s1 != null && s1.RemoteNodeID == e.Message.Sender && s.Event.EventDetail.DataType == type
                                  select s1).ToList().GroupBy(s => s.SubscriptionReference);

            IEnumerable<RemoteSubscriptionHandle> handles = Subscribe.RemoteSubscriber(type, e.Message.Sender,
                                                                                       (IPEndPoint)
                                                                                       _remoteHosts[e.Message.Sender],
                                                                                       e.Message.Id);


            if (handles != null)
            {
                //Send reply
                byte[] handledata = Serializer.Serialize(handles);
                byte[] reply = new byte[handledata.Length + 4];
                byte[] idbytes = BitConverter.GetBytes(e.Message.Id);
                Array.Copy(idbytes, reply, idbytes.Length);
                Array.Copy(handledata, 0, reply, idbytes.Length, handledata.Length);
                Message m = new Message { Type = MessageType.SubscribeResponse, Data = reply };
                IPEndPoint ep = (IPEndPoint)_remoteHosts[e.Message.Sender];
                _log.DebugFormat("Replying to subscription request at {0}", ep);
                Client.Send(m, ep.Address.ToString(), ep.Port);
            }
            foreach (var currentHandle in currentHandles)
            {
                //Send reply
                byte[] handledata = Serializer.Serialize(currentHandle.ToList());
                byte[] reply = new byte[handledata.Length + 4];
                byte[] idbytes = BitConverter.GetBytes(currentHandle.Key);
                Array.Copy(idbytes, reply, idbytes.Length);
                Array.Copy(handledata, 0, reply, idbytes.Length, handledata.Length);
                Message m = new Message { Type = MessageType.SubscribeResponse, Data = reply };
                IPEndPoint ep = (IPEndPoint)_remoteHosts[e.Message.Sender];
                _log.DebugFormat("Replying to subscription request at {0}", ep);
                Client.Send(m, ep.Address.ToString(), ep.Port);
            }
        }
        #endregion
        #region Publish
        private static void ProcessPublish(MessageEventArgs e)
        {
            /*
                                     * Remote publisher is identified by
                                     * Remote node ID (contained in the message object)
                                     * Remote publisher ID
                                     * Remote publisher-event ID
                                     * Assume shorts for all
                                     */
            short publisherID = BitConverter.ToInt16(e.Message.Data, 0);
            short eventID = BitConverter.ToInt16(e.Message.Data, 2);
            RemoteSubscriptionHandle handle = new RemoteSubscriptionHandle
            {
                EventID = eventID,
                PublisherId = publisherID,
                RemoteNodeID = e.Message.Sender,
            };
            byte[] data = new byte[e.Message.Data.Length - 4];
            Buffer.BlockCopy(e.Message.Data, 4, data, 0, data.Length);
            var subscriptions = from s in EllaModel.Instance.Subscriptions
                                let h = (s.Handle as RemoteSubscriptionHandle)
                                where h != null && h == handle
                                select s;
            foreach (var sub in subscriptions)
            {
                (sub.Event.Publisher as Stub).NewMessage(data);
            }
        }
        #endregion
    }
}
