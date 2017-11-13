//=============================================================================
// Project  : Ella Middleware
// File    : EventHandle.cs
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

namespace Ella.Model
{
    [Serializable]
    internal class EventHandle
    {
        protected bool Equals(EventHandle other)
        {
            return PublisherNodeId == other.PublisherNodeId && EventId == other.EventId && PublisherId == other.PublisherId;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = PublisherNodeId;
                hashCode = (hashCode*397) ^ EventId;
                hashCode = (hashCode*397) ^ PublisherId;
                return hashCode;
            }
        }

        /// <summary>
        /// Gets or sets the publisher node id.
        /// </summary>
        /// <value>
        /// The publisher node id.
        /// </value>
        internal int PublisherNodeId { get; set; }

        /// <summary>
        /// Gets or sets the event id.
        /// </summary>
        /// <value>
        /// The event id.
        /// </value>
        internal int EventId { get; set; }

        /// <summary>
        /// Gets or sets the publisher id.
        /// </summary>
        /// <value>
        /// The publisher id.
        /// </value>
        internal int PublisherId { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EventHandle) obj);
        }
    }
}
