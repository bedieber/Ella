using System;
using System.Collections.Generic;
using Ella.Control;

namespace Ella.Network
{
    //TODO think about where to place this interface

    /// <summary>
    /// Base interface for network controller classes to handle communication via an arbitrary network
    /// </summary>
    internal interface INetworkController
    {
        /// <summary>
        /// Subscribes to a remote host.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="callback"></param>
        void SubscribeTo(Type type, Action<RemoteSubscriptionHandle> callback);

        bool SendMessage(ApplicationMessage message, RemoteSubscriptionHandle remoteSubscriptionHandle, bool isReply = false);

        /// <summary>
        /// Unsubscribes a node from the subscription defined by the subscription reference.
        /// </summary>
        /// <param name="subscriptionReference">The subscription reference.</param>
        /// <param name="nodeId">The node id.</param>
        void UnsubscribeFrom(int subscriptionReference, int nodeId);

        /// <summary>
        /// Sends a shutdown message.
        /// </summary>
        void SendShutdownMessage();
        
        /// <summary>
        /// Initializes and starts the NetworkController
        /// </summary>
        void Start();
        
        /// <summary>
        /// Indicates if the network controller instance has been started
        /// </summary>
        bool IsRunning { get; }

        List<INetworkServer> Servers { get; }

        //TODO this method should be refactored when new networks are added. signature might not be appropriate for all types of multicast
        /// <summary>
        /// Connects to a multicast group
        /// </summary>
        /// <param name="group"></param>
        /// <param name="port"></param>
        void ConnectToMulticastGroup(string group, int port);

        /// <summary>
        /// Stops the network controller and all servers
        /// </summary>
        void Stop();

        void BroadcastMessage(Message msg);
    }
}