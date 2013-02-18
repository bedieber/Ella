using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ella.Model
{
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

        internal int PublisherNodeId { get; set; }

        internal int EventId { get; set; }

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
