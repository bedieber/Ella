using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ella
{
    /// <summary>
    /// Represents one specific subscription of one subscriber to one publisher
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
        /// <summary>
        /// Gets or sets the remote node ID.
        /// </summary>
        /// <value>
        /// The remote node ID.
        /// </value>
        internal int RemoteNodeID { get; set; }

        public override int GetHashCode()
        {
            int hashCode = PublisherId;
            hashCode = (hashCode * 397) ^ EventID;
            return (hashCode * 397) ^ RemoteNodeID;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}/{2}:{3}", RemoteNodeID, PublisherId, EventID, GetHashCode());
        }
    }

}
