using Ella.Network.Communication;

namespace Ella.Network
{
    /// <summary>
    /// Definition for the eventhandler which will be notified of new messages
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MessageEventArgs" /> instance containing the event data.</param>
    internal delegate void MessageEventHandler(object sender, MessageEventArgs e);
    internal interface INetworkServer
    {
        /// <summary>
        /// Occurs when a new message arrives.
        /// </summary>
        event MessageEventHandler NewMessage;

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();
    }
}