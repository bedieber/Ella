using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;

namespace Ella.Network.Communication
{
    /// <summary>
    /// A network server class used to listen on a port and reconstruct incoming messages
    /// </summary>
    internal class Server
    {

        private ILog _log = LogManager.GetLogger(typeof(Server));
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
        private Thread _tpcListenerThread;
        private Thread _udpListenerThread;
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
            _tpcListenerThread = new Thread((ThreadStart)delegate
                                                          {
                                                              TcpListener listener = new TcpListener(_address, _port);
                                                              listener.Start();

                                                              _log.DebugFormat("Server: Started");
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
                                                                  _log.DebugFormat("Server Listener thread interrupted");
                                                              }
                                                              catch (ThreadAbortException)
                                                              {
                                                                  _log.DebugFormat("Server Listener Thread aborted. Exiting");
                                                              }
                                                              catch (Exception e)
                                                              {
                                                                  _log.ErrorFormat("Exception in Server: {0}",
                                                                                    e.Message);
                                                              }
                                                              finally
                                                              {
                                                                  listener.Stop();
                                                              }
                                                          });
            _tpcListenerThread.Start();

            _udpListenerThread = new Thread((ThreadStart)delegate
            {
                //TODO port
                UdpClient listener = new UdpClient(44556);
                try
                {
                    while (true)
                    {
                        IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);

                        byte[] datagram = listener.Receive(ref ep);
                        IPEndPoint sender = ep;
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            ProcessUdpMessage
                                (datagram, ep);
                        });
                    }
                }
                catch (ThreadInterruptedException)
                {
                    _log.DebugFormat("Server Listener thread interrupted");
                }
                catch (ThreadAbortException)
                {
                    _log.DebugFormat("Server Listener Thread aborted. Exiting");
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Exception in Server: {0}",
                                      e.Message);
                }
                finally
                {

                }
            });
            _udpListenerThread.Start();

        }

        private void ProcessUdpMessage(byte[] datagram, IPEndPoint ep)
        {
            //TODO msg id: necessary? then put it into datagram
            Message msg = new Message(-1) { Data = datagram, Sender = Convert.ToInt32(datagram), Type = MessageType.Discover };
            if (NewMessage != null)
            {
                NewMessage(this, new MessageEventArgs(msg) { Address = ep });
            }
            else
                _log.DebugFormat("Server: No listeners for new messages attached");
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
                _log.ErrorFormat("Could not resolve nodeID for address {0}", addressString);
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
                NewMessage(this, new MessageEventArgs(m) { Address = client.Client.RemoteEndPoint });
            }
            else
                _log.DebugFormat("Server: No listeners for new messages attached");
            stream.Close();
            client.Close();
        }


        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            _log.DebugFormat("Stopping server");
            _tpcListenerThread.Interrupt();
            _tpcListenerThread.Join(1000);
            if (_tpcListenerThread.IsAlive)
            {
                _tpcListenerThread.Abort();
            }
            _udpListenerThread.Interrupt();
            _udpListenerThread.Join(1000);
            if (_tpcListenerThread.IsAlive)
            {
                _udpListenerThread.Abort();
            }


            _log.DebugFormat("Server stopped");
        }
    }
}