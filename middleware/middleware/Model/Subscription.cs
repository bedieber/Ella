namespace Ella.Model
{
    /// <summary>
    /// Describes one single subscription of one subscriber to one publisher
    /// </summary>
    public class Subscription
    {
        /// <summary>
        /// Gets or sets the subscriber.
        /// </summary>
        /// <value>
        /// The subscriber.
        /// </value>
        public object Subscriber { get; set; }

        /// <summary>
        /// Gets or sets the event associated with this subscription.
        /// </summary>
        /// <value>
        /// The event.
        /// </value>
        internal Event Event { get; set; }
    }
}