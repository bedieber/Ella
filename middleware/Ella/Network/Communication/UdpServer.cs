//=============================================================================
// Project  : Ella Middleware
// File    : UdpServer.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@uni-klu.ac.at)
// Copyright 2012 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace Ella.Network.Communication
{
    class UdpServer
    {
        private int _port;

        private Thread _udpListenerThread;

        private ILog _log = LogManager.GetLogger(typeof(UdpServer));

        public delegate void MessageEventHandler(object sender, MessageEventArgs e);
        public event MessageEventHandler NewMessage;

        internal List<IPEndPoint> EndPointsMulticastGroup = new List<IPEndPoint>();


        /// <summary>
        /// Initializes a new instance of the <see cref="UdpServer" /> class.
        /// </summary>
        /// <param name="port">The port.</param>
        public UdpServer(int port)
        {
            _port = port;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
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
                                (datagram, sender);
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
            });
            _udpListenerThread.Start();
        }

        internal void ConnectToMulticastGroup(string group, int port)
        {

            //Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //IPAddress groupIP = IPAddress.Parse(group);

            //EndPointsMulticastGroup.Add(endPoint);

            //sock.Bind(endPoint);
            //IPEndPoint groupEndPoint = new IPEndPoint(groupIP, port);
            //sock.Connect(groupEndPoint);

            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, port);

            if (EndPointsMulticastGroup.Contains(endpoint))
                return;

            EndPointsMulticastGroup.Add(endpoint);

            UdpClient client = new UdpClient(endpoint);

            IPEndPoint groupEp = new IPEndPoint(IPAddress.Parse(group), port);
            client.Connect(groupEp);

            while (true)
            {
                byte[] datagram = client.Receive(ref groupEp);

                ThreadPool.QueueUserWorkItem(delegate
                {
                    ProcessUdpMessage
                        (datagram, groupEp);
                });
            }

        }

        /// <summary>
        /// Processes the message.
        /// </summary>
        /// <param name="datagram">The datagram.</param>
        /// <param name="ep"> The IP Endpoint. </param>
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
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            _log.DebugFormat("Stopping udp server");

            _udpListenerThread.Interrupt();
            _udpListenerThread.Join(1000);
            if (_udpListenerThread.IsAlive)
            {
                _udpListenerThread.Abort();
            }

            _log.DebugFormat("Server stopped");
        }
    }
}
