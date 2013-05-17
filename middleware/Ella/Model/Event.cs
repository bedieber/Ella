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