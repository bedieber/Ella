using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Ella.Attributes;
using Ella.Exceptions;
using Ella.Internal;
using Ella.Model;
using Ella.Network;
using log4net;
using log4net.Config;
using log4net.Util;

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
        /// <exception cref="System.ArgumentException">If No valid starter was method found</exception>
        /// <remarks>
        /// A publisher must define a parameterless method attributed with <see cref="Ella.Attributes.StartAttribute" />
        /// </remarks>
        public static void Publisher(object instance)
        {
            _log.InfoFormat("Starting publisher {0}", instance);
            var type = instance.GetType();
            if (Is.ValidPublisher(type))
            {
                var method = ReflectionUtils.GetAttributedMethod(type, typeof(StartAttribute));
                if (method == null)
                {
                    _log.ErrorFormat("{0} does not define a start method", instance.GetType());
                    throw new InvalidPublisherException("Publisher does not define a start method");
                }
                if (!method.GetParameters().Any())
                {
                    EllaModel.Instance.AddActivePublisher(instance);
                    Thread t = new Thread(() => method.Invoke(instance, null));
                    t.Start();
                }
            }
            else
            {
                _log.ErrorFormat("{0} is not a valid publisher", instance.GetType());
                throw new InvalidPublisherException(instance.GetType().ToString());
            }
        }

        /// <summary>
        /// Starts the Ella network functionality<br />
        /// Unless this method is called, your Ella application will be local-only
        /// </summary>
        public static void Network()
        {
            _log.Info("Starting network controller");
            NetworkController.Start();

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

    /// <summary>
    /// This class is used to stop running publishers
    /// </summary>
    public static class Stop
    {
        private static ILog _log = LogManager.GetLogger(typeof(Stop));

        /// <summary>
        /// Stops a publisher.
        /// </summary>
        /// <param name="instance">The instance of a publisher to be stopped.</param>
        /// <exception cref="System.ArgumentException">If no valid stop method was found.</exception>
        /// <remarks>
        /// A publisher has to define a parameterless method attributed with <see cref="Ella.Attributes.StopAttribute" />
        /// </remarks>
        public static void Publisher(object instance)
        {
            _log.InfoFormat("Stopping publisher {0}", instance);
            var type = instance.GetType();
            if (Is.Publisher(type))
            {
                var method = ReflectionUtils.GetAttributedMethod(type, typeof(StopAttribute));

                if (method == null)
                {
                    _log.ErrorFormat("{0} does not define a stop method", instance.GetType());
                    throw new InvalidPublisherException("No valid stop method found");
                }

                if (!method.GetParameters().Any())
                {
                    method.Invoke(instance, null);
                    EllaModel.Instance.RemoveActivePublisher(instance);
                }
                else
                {
                    throw new InvalidPublisherException("Stop method requires parameters, this is not supported");
                }
            }
        }
    }
}
