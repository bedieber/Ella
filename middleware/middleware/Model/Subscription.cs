namespace Ella.Model
{
    /// <summary>
    /// Describes one single subscription of one subscriber to one publisher
    /// </summary>
    internal class Subscription<T> : SubscriptionBase
    {
        /// <summary>
        /// Gets or sets the subscriber.
        /// </summary>
        /// <value>
        /// The subscriber.
        /// </value>
        internal object Subscriber { get; set; }

        /// <summary>
        /// Gets or sets the event associated with this subscription.
        /// </summary>
        /// <value>
        /// The event.
        /// </value>
        internal Event Event { get; set; }

        internal System.Action<T> Callback { get; set; }
    }
}