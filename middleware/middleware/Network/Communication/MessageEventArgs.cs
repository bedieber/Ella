using System;

namespace Ella.Network.Communication
{
    /// <summary>
    /// A class used as an eventArgs for a new Network message
    /// </summary>
    internal class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public Message Message { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageEventArgs" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public MessageEventArgs(Message message)
        {
            this.Message = message;
        }
    }
}
