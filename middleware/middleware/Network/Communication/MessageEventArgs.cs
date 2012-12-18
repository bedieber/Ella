using System;
using System.Net;

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
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public EndPoint Address { get; set; }

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
