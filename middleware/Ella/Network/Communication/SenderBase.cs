using System;
using System.Net;
using System.Threading;

namespace Ella.Network.Communication
{
    internal abstract class SenderBase
    {

        private static Func<EndPoint, SenderBase> _factoryMethod = Create;

        internal static Func<EndPoint, SenderBase> FactoryMethod
        {
            set { _factoryMethod = value; }
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="m">The message to send</param>
        internal abstract void Send(Message m);

        /// <summary>
        /// Creates a sender according to the provided endpoint
        /// </summary>
        /// <param name="ep">The endpoint specifying the sender type to search</param>
        /// <returns>A sender</returns>
        internal static SenderBase CreateSender(EndPoint ep)
        {
            return _factoryMethod(ep);
        }

        private static SenderBase Create(EndPoint ep)
        {
            if (ep is IPEndPoint)
            {
                IPEndPoint ipEndPoint = ep as IPEndPoint;
                return new IpSender(ipEndPoint.Address.ToString(), ipEndPoint.Port);
            }
            return null;
        }

        /// <summary>
        /// Sends the specified Message <paramref name="m"/> asynchronously. This call returns immediately.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <param name="endPoint"></param>
        /// <param name="sendingFinishedCallback">The sending finished callback.</param>
        internal static void SendAsync(Message m, EndPoint endPoint, Action<int> sendingFinishedCallback = null)
        {
            new Thread((ThreadStart)delegate
            {
                SendMessage(m, endPoint);
                if (sendingFinishedCallback != null)
                {
                    sendingFinishedCallback(m.Id);
                }
            }).Start();
        }

        internal void SendAsync(Message m)
        {
            new Thread(() => Send(m)).Start();
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="m">The message to send</param>
        /// <param name="endPoint"></param>
        internal static void SendMessage(Message m, EndPoint endPoint)
        {
            SenderBase s = CreateSender(endPoint);
            s.Send(m);
        }
    }
}