using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Ella.Network.Communication
{
    /// <summary>
    /// The networking client used to contact a remote endpoint
    /// </summary>
    internal class Client
    {
        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        public static void Send(Message m, string address, int port)
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(IPAddress.Parse(address), port);
                NetworkStream stream = client.GetStream();
                byte[] serialize = m.Serialize();
                stream.Write(serialize, 0, serialize.Length);
                stream.Flush();
                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("NetworkClient: failed to send message {0} to {1}", m.Id,
                                  address, e.Message);
            }
        }

        /// <summary>
        /// Sends the specified Message <paramref name="m"/> asynchronously. This call returns immediately.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="sendingFinishedCallback">The sending finished callback.</param>
        public static void SendAsync(Message m, string address, int port, Action<int> sendingFinishedCallback)
        {
            new Thread((ThreadStart)delegate
            {
                Send(m, address, port);
                if (sendingFinishedCallback != null)
                {
                    sendingFinishedCallback(m.Id);
                }
            }).Start();
        }
    }
}