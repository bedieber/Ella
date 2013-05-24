//=============================================================================
// Project  : Ella Middleware
// File    : SubscriptionBase.cs
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
using System.Linq;
using System.Text;
using System.Threading;
using Ella.Data;

namespace Ella.Model
{
    /// <summary>
    /// Base class for subscriptions
    /// </summary>
    internal abstract class SubscriptionBase
    {
        private int _nextSubscriptionId = 0;
        private SubscriptionHandle _handle;

        protected SubscriptionBase()
        {
            SubscriptionID = Interlocked.Increment(ref _nextSubscriptionId);
        }
        /// <summary>
        /// Gets or sets the subscriber.
        /// </summary>
        /// <value>
        /// The subscriber.
        /// </value>
        internal object Subscriber { get; set; }

        /// <summary>
        /// Gets or sets the event associated with this subscription.
        /// </summary>
        /// <value>
        /// The event.
        /// </value>
        internal Event Event { get; set; }

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        internal Type DataType { get; set; }

        /// <summary>
        /// Gets or sets the modify policy which is used to indicate whether a subscriber modifies the data or not.
        /// </summary>
        /// <value>
        /// The ModifyPolicy.
        /// </value>
        internal DataModifyPolicy ModifyPolicy { get; set; }

        /// <summary>
        /// Gets or sets the handle.
        /// </summary>
        /// <value>
        /// The handle.
        /// </value>
        internal SubscriptionHandle Handle
        {
            get { return _handle; }
            set
            {
                _handle = value;
                //_handle.SubscriberId = SubscriptionID;
            }
        }

        /// <summary>
        /// Gets or sets the subscription ID.
        /// </summary>
        /// <value>
        /// The subscription ID.
        /// </value>
        internal int SubscriptionID { get; set; }
    }
}
