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
    }
}