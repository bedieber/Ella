//=============================================================================
// Project  : Ella Middleware
// File    : MessageEventArgs.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using System.Net;
using Ella.Network.Communication;

namespace Ella.Network
{
    /// <summary>
    /// A class used as an eventArgs for a new Networking message
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
