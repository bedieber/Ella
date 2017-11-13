//=============================================================================
// Project  : Ella Middleware
// File    : UdpServer.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: https://github.com/bedieber/Ella.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Ella.Internal;
using log4net;

namespace Ella.Network.Communication
{
    /// <summary>
    /// An udp server class used to listen on a port and receive messages sent over udp.
    /// </summary>
    internal class UdpServer : INetworkServer, IMulticastListener
    {
        private int _port;

        private Thread _udpListenerThread;

        private ILog _log = LogManager.GetLogger(typeof(UdpServer));

        public event MessageEventHandler NewMessage;

        private List<IPEndPoint> EndPointsMulticastGroup = new List<IPEndPoint>();

        private List<Thread> _multicastThreads = new List<Thread>();
        private bool _run;


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
            _log.DebugFormat("Starting UDP server on port {0}", _port);
            _udpListenerThread = new Thread((ThreadStart)delegate
            {
                UdpClient listener = new UdpClient(_port);
                try
                {
                    IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                    _run = true;
                    while (_run)
                    {
                        var asyncResult = listener.BeginReceive(null, null);

                        bool received = asyncResult.AsyncWaitHandle.WaitOne(2000);
                        if (!_run || !received)
                        {
                            continue;
                        }
                        byte[] datagram = listener.EndReceive(asyncResult, ref ep);
                        _log.Debug("Processing UDP Message");

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
                    _log.DebugFormat("UDP TcpServer Listener thread interrupted");
                }
                catch (ThreadAbortException)
                {
                    _log.DebugFormat("UDP TcpServer Listener Thread aborted. Exiting");
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Exception in UDP Server: {0}",
                                      e.Message);
                }
                _log.DebugFormat("UDP server stopping");

            });
            _udpListenerThread.Start();
        }


        /// <summary>
        /// Connects to a multicast group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="port">The port.</param>
        public void ConnectToMulticastGroup(string group, int port)
        {
            Thread t = new Thread((ThreadStart)delegate
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
                    _run = true;
                    while (_run)
                    {
                        var waitHandle = new ManualResetEvent(false);
                        var asyncResult = client.BeginReceive((e) => waitHandle.Set(), null);
                        bool received = !waitHandle.WaitOne(2000);
                        if (!_run || !received)
                        {
                            continue;
                        }
                        byte[] datagram = client.EndReceive(asyncResult, ref groupEp);

                        ThreadPool.QueueUserWorkItem(delegate
                            {
                                ProcessUdpMessage
                                    (datagram, groupEp);
                            });
                    }
                });
            t.Start();
            _multicastThreads.Add(t);

        }

        /// <summary>
        /// Processes the message.
        /// </summary>
        /// <param name="datagram">The datagram.</param>
        /// <param name="ep"> The IP Endpoint. </param>
        private void ProcessUdpMessage(byte[] datagram, IPEndPoint ep)
        {
            Message msg = new Message(-1) { Data = datagram, Sender = BitConverter.ToInt32(datagram, 0), Type = MessageType.Discover };
            _log.DebugFormat("New UDP message from {0}", msg.Sender);
            if (NewMessage != null)
            {
                NewMessage(this, new MessageEventArgs(msg) { Address = ep });
            }
            else
                _log.DebugFormat("TcpServer: No listeners for new messages found when processing UDP message");
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            _log.DebugFormat("Stopping udp server");

            StopThread(_udpListenerThread);

            foreach (var multicastThread in _multicastThreads)
            {
                StopThread(multicastThread);
            }
            _log.DebugFormat("UDP stopped");
        }

        /// <summary>
        /// Stops the thread.
        /// </summary>
        /// <param name="t">The thread</param>
        private void StopThread(Thread t)
        {
            t.Interrupt();
            t.Join(1000);
            if (t.IsAlive)
            {
                t.Abort();
            }
        }
    }
}
