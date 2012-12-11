using Ella.Attributes;

namespace Ella.Model
{
    /// <summary>
    /// Describes an event that can be published by a specific publisher instance
    /// </summary>
    internal class Event
    {
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
            if (obj is Event)
                return this.GetHashCode() == obj.GetHashCode();
            return base.Equals(obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return string.Format("{0}/{1}", Publisher.GetHashCode(), EventDetail.ID).GetHashCode();
        }
    }
}