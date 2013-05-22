//=============================================================================
// Project  : Ella Middleware
// File    : Event.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@uni-klu.ac.at)
// Copyright 2012 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using Ella.Attributes;

namespace Ella.Model
{
    /// <summary>
    /// Describes an event that can be published by a specific publisher instance
    /// </summary>
    internal class Event
    {
        protected bool Equals(Event other)
        {
            return Equals(Publisher, other.Publisher) && Equals(EventDetail, other.EventDetail);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Publisher != null ? Publisher.GetHashCode() : 0)*397) ^ (EventDetail != null ? EventDetail.GetHashCode() : 0);
            }
        }

        /// <summary>
        /// Gets or sets the publisher.
        /// </summary>
        /// <value>
        /// The publisher.
        /// </value>
        internal object Publisher { get; set; }

        /// <summary>
        /// Gets or sets the event detail.
        /// </summary>
        /// <value>
        /// The event detail.
        /// </value>
        internal PublishesAttribute EventDetail { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Event) obj);
        }

        //TODO introduce multicast address and multicast port (IPEndPoint)

    }
}