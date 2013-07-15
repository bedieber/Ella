namespace Ella.Network
{
    internal interface IMulticastListener
    {
        /// <summary>
        /// Connects to a multicast group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="port">The port.</param>
        void ConnectToMulticastGroup(string group, int port);
    }
}