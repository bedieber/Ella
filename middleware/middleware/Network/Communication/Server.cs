using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Ella.Network.Communication
{
    /// <summary>
    /// A network server class used to listen on a port and reconstruct incoming messages
    /// </summary>
    internal class Server
    {
        /// <summary>
        /// Definition for the eventhandler which will be notified of new messages
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MessageEventArgs" /> instance containing the event data.</param>
        public delegate void MessageEventHandler(object sender, MessageEventArgs e);
        /// <summary>
        /// Occurs when a new message arrives.
        /// </summary>
        public event MessageEventHandler NewMessage;

        private int _port;
        private IPAddress _address;
        private Thread _listenerThread;
        public Dictionary<int, string> NodeDictionary { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Server" /> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="address">The address.</param>
        public Server(int port, IPAddress address)
        {
            _port = port;
            _address = address;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            _listenerThread = new Thread((ThreadStart)delegate
                                                          {
                                                              TcpListener listener = new TcpListener(_address, _port);
                                                              listener.Start();
                                                              Console.WriteLine("Server: Started");
                                                              try
                                                              {
                                                                  while (true)
                                                                  {

                                                                      TcpClient client = listener.AcceptTcpClient();
                                                                      ThreadPool.QueueUserWorkItem(delegate
                                                                                                       {
                                                                                                           ProcessMessage
                                                                                                               (client);
                                                                                                       });
                                                                  }
                                                              }
                                                              catch (ThreadInterruptedException)
                                                              {
                                                                  Console.WriteLine("Server Listener thread interrupted");
                                                              }
                                                              catch (ThreadAbortException)
                                                              {
                                                                  Console.WriteLine("Server Listener Thread aborted. Exiting");
                                                              }
                                                              catch (Exception e)
                                                              {
                                                                  Console.WriteLine("Exception in Server: {0}",
                                                                                    e.Message);
                                                              }
                                                              finally
                                                              {
                                                                  listener.Stop();
                                                              }
                                                          });
            _listenerThread.Start();
        }

        /// <summary>
        /// Processes the message.
        /// </summary>
        /// <param name="client">The client.</param>
        private void ProcessMessage(TcpClient client)
        {

            /*
             * Message:
             * type 1 byte
             * id 4 bytes
             * length 4 bytes
             * data <length> bytes
             */
            NetworkStream stream = client.GetStream();
            short messageType = Convert.ToInt16(stream.ReadByte());
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, buffer.Length);
            int id = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[4];
            stream.Read(buffer, 0, buffer.Length);
            int length = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[length];
            byte[] data = new byte[length];
            int totalbytesRead = 0;
            string addressString = (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString();
            IEnumerable<int> sendernodes = (from n in NodeDictionary where n.Value == addressString select n.Key).DefaultIfEmpty(-1).ToArray();
            int senderid = -1;
            if (sendernodes == null || sendernodes.Count() == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Could not resolve nodeID for address {0}", addressString);
                Console.ResetColor();
            }
            else
            { senderid = sendernodes.FirstOrDefault(); }
            while (totalbytesRead < length)
            {
                int read = stream.Read(buffer, 0, buffer.Length);
                Array.Copy(buffer, 0, data, totalbytesRead, read);
                totalbytesRead += read;
            }
            if (NewMessage != null)
            {
                Message m = new Message(id) { Data = data, Type = ((MessageType)messageType), Sender = senderid };
                NewMessage(this, new MessageEventArgs(m){Address = client.Client.RemoteEndPoint});
            }
            else
                Console.WriteLine("Server: No listeners for new messages attached");
            stream.Close();
            client.Close();
        }


        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            Console.WriteLine("Stopping server");
            _listenerThread.Interrupt();
            _listenerThread.Join(1000);
            if (_listenerThread.IsAlive)
            {
                _listenerThread.Abort();
            }

            Console.WriteLine("Server stopped");
        }
    }
}