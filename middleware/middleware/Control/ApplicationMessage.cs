using System;

namespace Ella.Control
{
    /// <summary>
    /// This class encapsulates an application-defined message
    /// </summary>
    [Serializable]
    public class ApplicationMessage
    {
        /// <summary>
        /// Gets or sets the message payload
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public short MessageType { get; set; }

        /// <summary>
        /// Gets or sets the message id.
        /// </summary>
        /// <value>
        /// The message id.
        /// </value>
        public int MessageId { get; set; }

        /// <summary>
        /// Gets the message sender, this is automatically set by Ella upon sending a message.
        /// </summary>
        /// <value>
        /// The sender.
        /// </value>
        internal int Sender { get; set; }

        /// <summary>
        /// The subscriptionhandle
        /// </summary>
        internal SubscriptionHandle Handle { get; set; }

        public override string ToString()
        {
            return string.Format("ApplicationMessage {2} type {0} from {1}", MessageType, Sender, MessageId);
        }
    }
}
