//=============================================================================
// Project  : Ella Middleware
// File    : Stop.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@uni-klu.ac.at)
// Copyright 2012 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using System.Linq;
using System.Threading;
using Ella.Exceptions;
using Ella.Model;
using Ella.Network;
using log4net;

namespace Ella
{
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
        /// A publisher has to define a parameterless method attributed with <see cref="Attributes.StopAttribute" />
        /// </remarks>
        public static void Publisher(object instance)
        {
            if (instance == null) return;
            _log.InfoFormat("Stopping publisher {0}", instance);
            var type = instance.GetType();
            if (Is.Publisher(type))
            {
                var publisher = EllaModel.Instance.GetPublisher(instance);
                if (publisher == null)
                {
                    _log.DebugFormat("{0} was not found to be an active publisher", instance);
                    throw new InvalidPublisherException();
                }
                try
                {
                    publisher.StopMethod.Invoke(instance, null);
                }
                catch (Exception ex)
                {
                    _log.ErrorFormat("Stopping publisher {0} threw an exception: {1}", instance, ex.Message);
                }
                EllaModel.Instance.RemoveActivePublisher(publisher);
            }
            else
            {
                throw new InvalidPublisherException(string.Format("{0} is not a publisher", instance));
            }

        }

        /// <summary>
        /// Performs a clear termination of the Ella system<br />
        /// Includes
        /// <list type="bullet">
        /// <item>
        /// <description>Stopping all publishers </description>
        /// </item>
        /// <item>
        /// <description>Cancelling all subscriptions </description>
        /// </item>
        /// <item>
        /// <description>Notifying other nodes of the termination </description>
        /// </item>
        /// </list>
        /// </summary>
        public static void Ella()
        {
            _log.Debug("Shutting down Ella");

            _log.Debug("Broadcasting shutdown message to all nodes");
            /*
             * Notify other nodes of the termination
             */
            NetworkController.BroadcastShutdown();
            /*
             * Cancel all subscriptions
             */
            var groupedSubscriptions = EllaModel.Instance.Subscriptions.GroupBy(s => s.Subscriber).ToArray();
            foreach (var s in groupedSubscriptions)
            {
                Unsubscribe.From(s.Key);
            }
            _log.Debug("Unsubscribed all subscribers");
            /*
             * Stop all publishers
             */
            var activePublishers = EllaModel.Instance.GetActivePublishers().ToArray();
            _log.Debug("Stopping publishers");
            foreach (var activePublisher in activePublishers)
            {
                try
                {
                    Stop.Publisher(activePublisher);
                }
                catch (Exception ex)
                {
                    _log.ErrorFormat("Could not stop publisher {0}. {1}", activePublisher, ex.Message);
                }
            }

            //Join and terminate (if necessary) the publisher threads
            foreach (Thread t in EllaModel.Instance.PublisherThreads)
            {
                if (!t.Join(1000))
                {
                    try
                    {
                        t.Abort();
                        if (!t.Join(1000))
                            _log.DebugFormat("Could not terminate thread {0}. Did all I could.", t);
                    }
                    catch (Exception)
                    {
                        _log.DebugFormat("Could not terminate thread {0}. Did all I could.", t);
                    }
                }
            }

        }
    }
}