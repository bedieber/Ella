//=============================================================================
// Project  : Ella Middleware
// File    : Start.cs
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
using System.Net;
using System.Threading;
using Ella.Controller;
using Ella.Exceptions;
using Ella.Internal;
using Ella.Model;
using Ella.Network;
using Ella.Network.Communication;
using log4net;
using System.Linq;

namespace Ella
{
    /// <summary>
    /// This class is used to start publishers
    /// </summary>
    public static class Start
    {
        private static ILog _log = LogManager.GetLogger(typeof(Start));
        /// <summary>
        /// Starts the publisher.
        /// </summary>
        /// <param name="instance">The instance of a publisher to be started.</param>
        /// <exception cref="System.ArgumentException">If no valid starter was method found</exception>
        /// <remarks>
        /// A publisher must define a parameterless method attributed with <see cref="Attributes.StartAttribute" />
        /// </remarks>
        public static void Publisher(object instance)
        {
            Publisher publisher = ReflectionUtils.CreatePublisher(instance);
            if (publisher == null)
                throw new InvalidPublisherException(string.Format("{0} is not a valid publisher", instance));
            EllaModel.Instance.AddActivePublisher(publisher);
            _log.InfoFormat("Starting publisher {0}", EllaModel.Instance.GetPublisherId(instance));
            Thread t = new Thread(() => publisher.StartMethod.Invoke(instance, null));
            EllaModel.Instance.PublisherThreads.Add(t);
            t.Start();
            foreach (var publishedEvent in publisher.Events)
            {
                var subscriptionRequests = EllaModel.Instance.SubscriptionRequests.Where(sr => sr.RequestedType == publishedEvent.EventDetail.DataType).ToList();
                foreach (var subscriptionRequest in subscriptionRequests)
                {
                    _log.DebugFormat("Late-subscribing subscriber {0} of new publisher {1} for requested type {2}", subscriptionRequest.SubscriberInstance, instance, subscriptionRequest.RequestedType);
                    try
                    {
                        subscriptionRequest.SubscriptionCall.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _log.ErrorFormat("Could not invoke subscription call for subscriber {0}. {1}", subscriptionRequest.SubscriberInstance, ex.Message);
                    }                    
                }
            }
        }

        /// <summary>
        /// Starts the Ella network functionality<br />
        /// Unless this method is called, your Ella application will be local-only
        /// </summary>
        public static void Network()
        {
            _log.Info("Starting network controller");
            Networking.NetworkController = new NetworkController();
            Networking.NetworkController.Servers.Add(new UdpServer(EllaConfiguration.Instance.NetworkPort));
            Networking.NetworkController.Servers.Add(new TcpServer(EllaConfiguration.Instance.NetworkPort, IPAddress.Any));
            global::Ella.Networking.Start();
        }

        /// <summary>
        /// Initializes the Ella Middleware system
        /// </summary>
        public static void Ella()
        {
            //  XmlConfigurator.ConfigureAndWatch(new FileInfo(
            //Path.GetDirectoryName(
            //      Assembly.GetAssembly(typeof(Start)).Location)
            //     + @"\" + "Ella.dll.config"));
            //  _log.Info("Ella started");
        }


    }
}
