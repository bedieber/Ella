using System;
using System.Net;
using Ella.Control;
using Ella.Internal;
using Ella.Network.Communication;
using log4net;

namespace Ella.Network
{
    internal partial class NetworkController
    {
        private static readonly NetworkController _instance = new NetworkController();
        private static ILog _log = LogManager.GetLogger(typeof(NetworkController));


        internal static bool IsRunning { get { return _instance._server != null; } }

        /// <summary>
        /// Starts the network controller.
        /// </summary>
        internal static void Start()
        {
            _instance._server = new Server(EllaConfiguration.Instance.NetworkPort, IPAddress.Any);
            _instance._server.NewMessage += _instance.NewMessage;
            _instance._server.Start();
            Client.Broadcast();
        }
        /// <summary>
        /// Subscribes to remote host.
        /// </summary>
        /// <typeparam name="T">The type to subscribe to</typeparam>
        internal static void SubscribeToRemoteHost<T>(Action<RemoteSubscriptionHandle> callback)
        {
            _instance.SubscribeTo(typeof(T), callback);
        }

        internal static bool SendApplicationMessage(ApplicationMessage message, RemoteSubscriptionHandle remoteSubscriptionHandle, bool isReply = false)
        {
            return _instance.SendMessage(message, remoteSubscriptionHandle,isReply);
        }

        internal static void Unsubscribe(int subscriptionReference, int nodeId)
        {
            _instance.UnsubscribeFrom(subscriptionReference, nodeId);
        }

        internal static void BroadcastShutdown()
        {
            _instance.SendShutdownMessage();
        }
    }
}
