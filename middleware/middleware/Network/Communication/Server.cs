using System;
using System.Collections.Generic;
using System.IO.Compression;
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
        //TODO remove NodeDictionary
        public Dictionary<int, string> NodeDictionary { get; set; }

        internal List<IPEndPoint> EndPointsMulticastGroup = new List<IPEndPoint>();

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
            _log.InfoFormat("Starting TCP Server on Port {0}, with listener attached: {1}", _port, NewMessage!=null);
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
                                                                  _log.DebugFormat("TCP Server Listener thread interrupted");
                                                              }
                                                              catch (ThreadAbortException)
                                                              {
                                                                  _log.DebugFormat("TCP Server Listener Thread aborted. Exiting");
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
                UdpClient listener = new UdpClient(_port);
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
                    _log.DebugFormat("UDP Server Listener thread interrupted");
                }
                catch (ThreadAbortException)
                {
                    _log.DebugFormat("UDP Server Listener Thread aborted. Exiting");
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

        internal void ConnectToMulticastGroup(string group, int port)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);

            if(EndPointsMulticastGroup.Contains(endPoint))
                return;

            byte[] datagram = new byte[2048];

            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress groupIP = IPAddress.Parse(group);

            EndPointsMulticastGroup.Add(endPoint);

            sock.Bind(endPoint);
            IPEndPoint groupEndPoint = new IPEndPoint(groupIP, port);
            sock.Connect(groupEndPoint);

            while (true)
            {
                sock.Receive(datagram);

                ThreadPool.QueueUserWorkItem(delegate
                {
                    ProcessUdpMessage
                        (datagram, groupEndPoint);
                });
            }


        }

        private void ProcessUdpMessage(byte[] datagram, IPEndPoint ep)
        {
            Message msg = new Message(-1) { Data = datagram, Sender = BitConverter.ToInt32(datagram, 0), Type = MessageType.Discover };
            if (NewMessage != null)
            {
                NewMessage(this, new MessageEventArgs(msg) { Address = ep });
            }
            else
                _log.DebugFormat("Server: No listeners for new messages found when processing UDP message");
        }


        /// <summary>
        /// Processes the message.
        /// </summary>
        /// <param name="client">The client.</param>
        private void ProcessMessage(TcpClient client)
        {
            GZipStream stream = new GZipStream(client.GetStream(), CompressionMode.Decompress);

            try
            {
                if (NewMessage == null)
                {
                    _log.DebugFormat("Server: No listeners for new messages found when processing TCP message");
                    return;
                }

                /*
                    * Message:
                    * type 1 byte
                    * id 4 bytes
                    * sender 4 bytes
                    * length 4 bytes
                    * data <length> bytes
                    */

                //Type
                short messageType = Convert.ToInt16(stream.ReadByte());

                //id
                byte[] buffer = new byte[4];
                stream.Read(buffer, 0, buffer.Length);
                int id = BitConverter.ToInt32(buffer, 0);

                //sender
                buffer = new byte[4];
                stream.Read(buffer, 0, buffer.Length);
                int sender = BitConverter.ToInt32(buffer, 0);

                //length
                buffer = new byte[4];
                stream.Read(buffer, 0, buffer.Length);
                int length = BitConverter.ToInt32(buffer, 0);
                byte[] data = new byte[0];
                if (length > 0)
                {
                    //data
                    buffer = new byte[length];
                    data = new byte[length];

                    int totalbytesRead = 0;
                    string addressString = (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString();

                    while (totalbytesRead < length)
                    {
                        int read = stream.Read(buffer, 0, buffer.Length);
                        if (read == 0)
                        {
                            _log.Debug("0 bytes read, cancelling reception operation");
                            return;
                        }
                        Array.Copy(buffer, 0, data, totalbytesRead, read);
                        totalbytesRead += read;
                    }
                }

                Message m = new Message(id) { Data = data, Type = ((MessageType)messageType), Sender = sender };
                NewMessage(this, new MessageEventArgs(m) { Address = client.Client.RemoteEndPoint });

            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Exception during processing network message: {0}\n{1}", ex.Message, ex.StackTrace);
                throw;
            }
            finally
            {
                stream.Close();
                client.Close();
            }
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