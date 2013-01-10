using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ella
{
    /// <summary>
    /// Represents one specific subscription of one subscriber to one publisher<br />
    /// Comparing two subscription handles is guaranteed to evaluate to <c>true</c> if both handles describe the same subscription
    /// </summary>
    [Serializable]
    public class SubscriptionHandle
    {

        internal int PublisherId { get; set; }
        internal int EventID { get; set; }
        internal int SubscriberId { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SubscriptionHandle)obj);
        }

        protected bool Equals(SubscriptionHandle other)
        {
            return GetHashCode() == other.GetHashCode();
        }

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

        public static bool operator ==(SubscriptionHandle one, SubscriptionHandle two)
        {
            if (ReferenceEquals(one, null))
                return false;
            return one.Equals((object)two);
        }

        public static bool operator !=(SubscriptionHandle one, SubscriptionHandle two)
        {
            return !(one == two);
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
        internal int RemoteNodeID { get; set; }

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


        public override int GetHashCode()
        {
            int hashCode = PublisherId;
            hashCode = (hashCode * 397) ^ EventID;
            hashCode = (hashCode * 397) ^ SubscriptionReference;
            return (hashCode * 397) ^ RemoteNodeID;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}/{2}:{3}", RemoteNodeID, PublisherId, EventID, GetHashCode());
        }
    }

}
