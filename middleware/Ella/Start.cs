//=============================================================================
// Project  : Ella Middleware
// File    : Start.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: https://github.com/bedieber/Ella.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using Ella.Controller;
using Ella.Exceptions;
using Ella.Internal;
using Ella.Model;
using Ella.Network;
using Ella.Network.Communication;
using log4net;
using System.Linq;
using log4net.Appender;
using log4net.Config;
using log4net.Repository;
using log4net.Repository.Hierarchy;

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
            _log.InfoFormat("Starting publisher {0} of type {1}", EllaModel.Instance.GetPublisherId(instance), instance.GetType().Name);
            Thread t = new Thread(() => publisher.StartMethod.Invoke(instance, null));
            EllaModel.Instance.PublisherThreads.Add(t);
            t.Start();
            foreach (var publishedEvent in publisher.Events)
            {
                var subscriptionRequests = EllaModel.Instance.FilterSubscriptionRequests(sr => sr.RequestedType == publishedEvent.EventDetail.DataType).ToList();
                foreach (var subscriptionRequest in subscriptionRequests)
                {
                    _log.DebugFormat("Late-subscribing subscriber {0} of new publisher {1} for requested type {2}", subscriptionRequest.SubscriberInstance, instance, subscriptionRequest.RequestedType);
                    try
                    {
                        subscriptionRequest.SubscriptionCall.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _log.ErrorFormat("Could not invoke late-subscription delegate for subscriber {0}. {1}, {2}", subscriptionRequest.SubscriberInstance, ex.Message, ex.InnerException == null ? string.Empty : ex.InnerException.Message);
                    }
                }
            }
            if (Networking.IsRunning && !(instance is Stub))
            {
                foreach (var ev in publisher.Events)
                {
                    Message msg = new Message();
                    msg.Data = SerializationHelper.Serialize(ev.EventDetail.DataType);
                    msg.Type = MessageType.NewPublisher;
                    Networking.BroadcastMessage(msg);
                }
            }
        }

        /// <summary>
        /// Starts the Ella network functionality<br />
        /// Unless this method is called, your Ella application will be local-only
        /// </summary>
        /// <param name="initialHostlist">A list of already known hosts. The keys are host ids. The values are string representations ("ip:port") of the corresponding host. </param>
        public static void Network(IEnumerable<KeyValuePair<int, string>> initialHostList = null)
        {
            _log.Info("Starting network controller");
            Networking.NetworkController = new NetworkController(initialHostList);
            Networking.NetworkController.Servers.Add(new UdpServer(EllaConfiguration.Instance.NetworkPort));
            IPAddress listenAddress = !string.IsNullOrEmpty(EllaConfiguration.Instance.BindAddress) &&
                                      EllaConfiguration.Instance.BindAddress != "0.0.0.0"
                ? IPAddress.Parse(EllaConfiguration.Instance.BindAddress)
                : IPAddress.Any;
            Networking.NetworkController.Servers.Add(new TcpServer(EllaConfiguration.Instance.NetworkPort, listenAddress));
            global::Ella.Networking.Start();
        }

        /// <summary>
        /// Initializes the Ella Middleware system
        /// </summary>
        public static void Ella()
        {
            //if (!LogManager.GetRepository(Assembly.GetAssembly(typeof(Start))).Configured)
            //{
            var loggerRepository = LogManager.CreateRepository(Assembly.GetAssembly(typeof(Start)), typeof(Hierarchy));

            var configFile = new FileInfo(
                Path.GetDirectoryName(
                    Assembly.GetAssembly(typeof(Start)).Location)
                + @"\" + "Ella.dll.config");
            if (File.Exists(configFile.FullName))
            {
                XmlConfigurator.ConfigureAndWatch(loggerRepository, configFile);
            }
            else
            {
                ConsoleAppender appender = new ConsoleAppender();
                BasicConfigurator.Configure(appender);
            }
            _log.Info("Ella started");
            //}
        }


    }
}
