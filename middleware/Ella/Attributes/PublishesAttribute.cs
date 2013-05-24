//=============================================================================
// Project  : Ella Middleware
// File    : PublishesAttribute.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using Ella.Data;

namespace Ella.Attributes
{
    /// <summary>
    /// Attribute used to declare a publisher
    /// </summary>
    /// <remarks>For each publish event, a publisher must define one Publishes attribute. The <see cref="PublishesAttribute.ID"/> property defines a local ID for this event. No two events of a single publisher may have the same number (however, if two distinct publishers define the same ID, this is allowed)</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PublishesAttribute : Attribute
    {

        #region Private Members
        private Type _dataType;
        private int _id;
        #endregion

        #region Public Properties
        /// <summary>
        /// The data type published by this event
        /// </summary>
        public Type DataType { get { return _dataType; } }
        /// <summary>
        /// The local ID of this publishing event
        /// </summary>
        public int ID { get { return _id; } }

        /// <summary>
        /// Indicates if the data of the event should be transported to subscribers using a reliable channel or if a certain amount of loss can be tolerated<br />
        /// <c>True</c> by default
        /// </summary>
        public bool NeedsReliableTransport { get; set; }

        /// <summary>
        /// Gets or sets the copy policy, default is <see cref="DataCopyPolicy.None"/>.
        /// </summary>
        /// <value>
        /// The copy policy to be used when processing the published data.
        /// </value>
        public DataCopyPolicy CopyPolicy { get; set; }
        #endregion

        /// <summary>
        /// Creates a new PublishesAttribute
        /// </summary>
        /// <param name="dataType">The data type that will be published</param>
        /// <param name="id">The internal ID</param>
        public PublishesAttribute(Type dataType, int id)
        {
            _dataType = dataType;
            _id = id;
            NeedsReliableTransport = true;
            //CopyPolicy = policy;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PublishesAttribute)obj);
        }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        protected bool Equals(PublishesAttribute other)
        {
            return Equals(_dataType, other._dataType) && _id == other._id && CopyPolicy == other.CopyPolicy;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_dataType != null ? _dataType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _id;
                hashCode = (hashCode * 397) ^ (int)CopyPolicy;
                return hashCode;
            }
        }

    }
}
