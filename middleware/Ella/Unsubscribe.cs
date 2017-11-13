//=============================================================================
// Project  : Ella Middleware
// File    : Unsubscribe.cs
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
using System.Linq;
using System.Text;
using Ella.Controller;
using Ella.Data;
using Ella.Internal;
using Ella.Model;
using Ella.Network;
using log4net;

namespace Ella
{
    /// <summary>
    /// This class allows unsubscribing from existing subscriptions.
    /// </summary>
    public static class Unsubscribe
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Subscribe));


        /// <summary>
        /// Unsubscribes the <paramref name="subscriberInstance" /> from type <typeparamref name="T" />
        /// </summary>
        /// <typeparam name="T">The type to unsubscribe from</typeparam>
        /// <exception cref="System.ArgumentException">subscriberInstance must be a valid subscriber</exception>
        public static void From<T>(object subscriberInstance)
        {
            _log.DebugFormat("Unsubscribing {0} from type {1}", EllaModel.Instance.GetSubscriberId(subscriberInstance), typeof(T));


            if (!Is.Subscriber(subscriberInstance.GetType()))
            {
                _log.ErrorFormat("Cannot unsubscribe. {0} is not a valid subscriber", subscriberInstance.GetType().ToString());
                throw new ArgumentException("subscriberInstance must be a valid subscriber");
            }

            SubscriptionController.PerformUnsubscribe(s => s.Subscriber == subscriberInstance && s.Event.EventDetail.DataType == typeof(T));
        }

        /// <summary>
        /// Unsubscribes the <paramref name="subscriberInstance" /> by SubscriptionHandle <paramref name="handle" />
        /// </summary>
        /// <param name="subscriberInstance">The instance of a subscriber to be unsubscribed</param>
        /// <param name="handle"> The SubscriptionHandle by which subscriptions should be removed </param>
        /// <exception cref="System.ArgumentException">subscriberInstance must be a valid subscriber</exception>
        public static void From(object subscriberInstance, SubscriptionHandle handle)
        {
            _log.DebugFormat("Unsubscribing {0} from handle {1}", EllaModel.Instance.GetSubscriberId(subscriberInstance), handle);


            if (!Is.Subscriber(subscriberInstance.GetType()))
            {
                _log.ErrorFormat("Cannot unsubscribe. {0} is not a valid subscriber", subscriberInstance.GetType().ToString());
                throw new ArgumentException("subscriberInstance must be a valid subscriber");
            }

            SubscriptionController.PerformUnsubscribe(s => s.Handle == handle);
        }

        /// <summary>
        /// Unsubscribes the <paramref name="subscriberInstance"/> from all events
        /// </summary>
        /// <param name="subscriberInstance">The subscriber instance.</param>
        public static void From(object subscriberInstance)
        {
            _log.DebugFormat("Unsubscribing {0} from all events", EllaModel.Instance.GetSubscriberId(subscriberInstance));


            if (!Is.Subscriber(subscriberInstance.GetType()))
            {
                _log.ErrorFormat("Cannot unsubscribe. {0} is not a valid subscriber", subscriberInstance.GetType().ToString());
                throw new ArgumentException("subscriberInstance must be a valid subscriber");
            }
            SubscriptionController.PerformUnsubscribe(s => s.Subscriber == subscriberInstance);
        }
    }
}
