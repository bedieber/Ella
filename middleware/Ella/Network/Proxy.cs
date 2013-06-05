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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Ella.Attributes;
using Ella.Data;
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
        internal Sender Sender { get; set; }
        internal MulticastSender MulticastSender { get; set; }

        /// <summary>
        /// Creates the instance.
        /// </summary>
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
                Send(m);
            }
            else
            {
                _log.ErrorFormat("Object {0} of Event {1} is not serializable", data, handle);
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
                MulticastSender.Send(m);
            }
            else
            {
                Sender.Send(m);
            }
        }
    }
}
