//=============================================================================
// Project  : Ella Middleware
// File    : Proxy.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using System.Reflection;
using Ella.Attributes;
using Ella.Internal;
using Ella.Model;
using Ella.Network.Communication;
using log4net;

namespace Ella.Network
{
    /// <summary>
    /// A proxy is used to catch local events and transfer them to a remote subscriber stub
    /// </summary>
    [Subscriber()]
    internal class Proxy
    {
        private ILog _log = LogManager.GetLogger(typeof(Proxy));
        internal Event EventToHandle { get; set; }
        internal IpSender IpSender { get; set; }
        internal UdpSender UdpSender { get; set; }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        [Factory]
        internal Proxy()
        {
        }

        ~Proxy()
        {
            IpSender.Dispose();
            IpSender = null;
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
            try
            {
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
                    _log.DebugFormat("Sending message with data type {0} to remote stub", data.GetType().Name);

                    Send(m);
                }
                else
                {
                    _log.ErrorFormat("Object {0} of Event {1} is not serializable", data, handle);
                }
            }
            catch (Exception ex)
            {
                _log.FatalFormat("Could not send {0}: {1}", data.GetType(), ex.Message);
                throw;
            }

        }

        /// <summary>
        /// Sends the specified message
        /// </summary>
        /// <param name="m">The méssage</param>
        protected virtual void Send(Message m)
        {
            if (!EventToHandle.EventDetail.NeedsReliableTransport && m.Data.Length + 12 <= EllaConfiguration.Instance.MTU)
            {
                UdpSender.Send(m);
            }
            else
            {
                if (IpSender.EnqueueMessage(m))
                {
                    //Notify publisher that buffer had to be cleared. This indicates a slow communication channel or a too high publishing frequency
                    object publisher = EventToHandle.Publisher;
                    int eventId = EventToHandle.EventDetail.ID;
                    _log.DebugFormat("Event {0} of publisher {1} is congested.", eventId, publisher);
                    string callback = EventToHandle.EventDetail.CongestionCallback;
                    if (callback == null)
                        return;
                    MethodInfo info = publisher.GetType().GetMethod(callback);

                    if (info != null)
                    {
                        if (info.GetParameters().Length == 1 && info.GetParameters()[0].ParameterType == typeof(int))
                        {
                            object[] parameters = new object[] { eventId };
                            info.Invoke(publisher, parameters);
                        }
                        else
                        {
                            _log.ErrorFormat("Cannot call congestioncallback on {0} due to an invalid method signature. must be (int)", publisher);
                        }
                    }
                    else
                    {
                        _log.WarnFormat("No suitable congestion callback found on type {0}", publisher);
                    }

                }
            }
        }
    }
}
