using System;
using Ella.Control;
using Ella.Network;
using Ella.Network.Communication;
using log4net;

namespace Ella
{
    internal class Networking
    {
        private static ILog _log = LogManager.GetLogger(typeof(IpNetworkController));
        internal static bool IsRunning { get { return NetworkController.IsRunning; } }

        internal static INetworkController NetworkController { get; set; }

        /// <summary>
        /// Starts the network controller.
        /// </summary>
        internal static void Start()
        {
            Sender.Broadcast();
        }

        /// <summary>
        /// Subscribes to remote host.
        /// </summary>
        /// <typeparam name="T">The type to subscribe to</typeparam>
        internal static void SubscribeToRemoteHost<T>(Action<RemoteSubscriptionHandle> callback)
        {
            NetworkController.SubscribeTo(typeof(T), callback);
        }

        /// <summary>
        /// Sends the application message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="remoteSubscriptionHandle">The remote subscription handle.</param>
        /// <param name="isReply">if set to <c>true</c>, this is a reply to another message.</param>
        /// <returns></returns>
        internal static bool SendApplicationMessage(ApplicationMessage message, RemoteSubscriptionHandle remoteSubscriptionHandle, bool isReply = false)
        {
            return NetworkController.SendMessage(message, remoteSubscriptionHandle,isReply);
        }

        /// <summary>
        /// Unsubscribes the specified node from the subscription defined by the subscription reference.
        /// </summary>
        /// <param name="subscriptionReference">The subscription reference.</param>
        /// <param name="nodeId">The node id.</param>
        internal static void Unsubscribe(int subscriptionReference, int nodeId)
        {
            NetworkController.UnsubscribeFrom(subscriptionReference, nodeId);
        }

        /// <summary>
        /// Broadcasts the shutdown.
        /// </summary>
        internal static void BroadcastShutdown()
        {
            NetworkController.SendShutdownMessage();
        }

        /// <summary>
        /// Connects to multicast group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="port">The port.</param>
        internal static void ConnectToMulticast(string group, int port)
        {
            NetworkController.ConnectToMulticastGroup(@group,port);
        }
    }
}