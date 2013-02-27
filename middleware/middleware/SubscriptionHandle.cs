using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ella.Internal;
using Ella.Model;

namespace Ella
{
    /// <summary>
    /// Represents one specific subscription of one subscriber to one publisher<br />
    /// Comparing two subscription handles is guaranteed to evaluate to <c>true</c> if both handles describe the same subscription
    /// </summary>
    [Serializable]
    public class SubscriptionHandle
    {

        internal int PublisherId { get { return EventHandle.PublisherId; } set { EventHandle.PublisherId = value; } }
        internal int EventID { get { return EventHandle.EventId; } set { EventHandle.EventId = value; } }
        internal int SubscriberId { get; set; }

        internal EventHandle EventHandle { get; set; }

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
            return Equals((SubscriptionHandle)obj);
        }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        protected bool Equals(SubscriptionHandle other)
        {
            return GetHashCode() == other.GetHashCode();
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
                int hashCode = PublisherId;
                hashCode = (hashCode * 397) ^ EventID;
                hashCode = (hashCode * 397) ^ SubscriberId;
                return hashCode;
            }
        }

        /// <summary>
        /// Operator overload for ==
        /// </summary>
        /// <param name="one">The one.</param>
        /// <param name="two">The two.</param>
        /// <returns></returns>
        public static bool operator ==(SubscriptionHandle one, SubscriptionHandle two)
        {
            if (ReferenceEquals(one, two))
                return true;
            if (ReferenceEquals(one, null) && ReferenceEquals(two, null))
                return true;
            if (ReferenceEquals(one, null))
                return false;
            return one.Equals((object)two);
        }

        /// <summary>
        /// operator overload for !=
        /// </summary>
        /// <param name="one">The one.</param>
        /// <param name="two">The two.</param>
        /// <returns></returns>
        public static bool operator !=(SubscriptionHandle one, SubscriptionHandle two)
        {
            return !(one == two);
        }

        public override string ToString()
        {
            return string.Format("S{0}/P{1}/E{2}:H{3}", SubscriberId, PublisherId, EventID, GetHashCode());
        }

        public SubscriptionHandle()
        {
            EventHandle = new EventHandle() {PublisherNodeId = EllaConfiguration.Instance.NodeId};
        }
    }

    /// <summary>
    /// Identifies a subscription to a remote publisher
    /// </summary>
    [Serializable]
    internal class RemoteSubscriptionHandle : SubscriptionHandle
    {
        [NonSerialized]
        private int _subscriptionReference;


        /// <summary>
        /// Gets or sets the remote node ID.
        /// </summary>
        /// <value>
        /// The remote node ID.
        /// </value>
        internal int PublisherNodeID { get { return EventHandle.PublisherNodeId; } set { EventHandle.PublisherNodeId = value; } }

        internal int SubscriberNodeID { get; set; }
        /// <summary>
        /// Gets or sets the subscription reference.<br />
        /// This is the message ID with which the subscription was made
        /// </summary>
        /// <value>
        /// The subscription reference.
        /// </value>
        internal int SubscriptionReference
        {
            get { return _subscriptionReference; }
            set { _subscriptionReference = value; }
        }


        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            int hashCode = PublisherId;
            hashCode = (hashCode * 397) ^ EventID;
            hashCode = (hashCode * 397) ^ SubscriptionReference;
            hashCode = (hashCode * 397) ^ SubscriberNodeID;
            return (hashCode * 397) ^ PublisherNodeID;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("S{0}/PN{1}/P{2}/E{3}:H{4}", SubscriberId, PublisherNodeID, PublisherId, EventID, GetHashCode());
        }
    }

}
