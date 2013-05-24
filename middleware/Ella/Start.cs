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

using System.Threading;
using Ella.Attributes;
using Ella.Exceptions;
using Ella.Internal;
using Ella.Model;
using Ella.Network;
using log4net;

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
}
