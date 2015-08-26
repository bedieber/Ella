//=============================================================================
// Project  : Ella Middleware
// File    : TcpServer.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Ella.Internal;
using log4net;

namespace Ella.Network.Communication
{
    /// <summary>
    /// A network server class used to listen on a port and reconstruct incoming messages
    /// </summary>
    internal class TcpServer : INetworkServer
    {

        private ILog _log = LogManager.GetLogger(typeof(TcpServer));

        /// <summary>
        /// Occurs when a new message arrives.
        /// </summary>
        public event MessageEventHandler NewMessage;

        private int _port;
        private IPAddress _address;
        private Thread _tpcListenerThread;
        private bool _stopReading;

        //TODO remove NodeDictionary
        public Dictionary<int, string> NodeDictionary { get; set; }

        private Queue<TcpClient> _tasks = new Queue<TcpClient>();
        private Mutex _taskMutex = new Mutex();
        private ManualResetEvent _taskResetEvent = new ManualResetEvent(false);
        private List<Thread> _workerThreads = new List<Thread>(50);

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer" /> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="address">The address.</param>
        public TcpServer(int port, IPAddress address)
        {
            _port = port;
            _address = address;
            for (int i = 0; i < 50; i++)
            {
                Thread t = new Thread(RunWorker);
                _workerThreads.Add(t);
                t.Start();
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            _log.InfoFormat("Starting TCP TcpServer on Port {0}, with listener attached: {1}.", _port, NewMessage != null);
            _tpcListenerThread = new Thread((ThreadStart)delegate
                                                          {
                                                              TcpListener listener = new TcpListener(_address, _port);
                                                              listener.Start();

                                                              _log.DebugFormat("TcpServer: Started");
                                                              try
                                                              {
                                                                  while (true)
                                                                  {
                                                                      var asyncResult = listener.BeginAcceptTcpClient(null, null);

                                                                      while (!asyncResult.AsyncWaitHandle.WaitOne(2000))
                                                                      { }
                                                                      TcpClient client =
                                                                          listener.EndAcceptTcpClient(asyncResult);
                                                                      //ThreadPool.QueueUserWorkItem(delegate
                                                                      //                                 {
                                                                      //                                     ProcessMessage
                                                                      //                                         (client);
                                                                      //                                 });
                                                                      _taskMutex.WaitOne();
                                                                      _tasks.Enqueue(client);
                                                                      _taskMutex.ReleaseMutex();
                                                                      _taskResetEvent.Set();
                                                                  }
                                                              }
                                                              catch (ThreadInterruptedException)
                                                              {
                                                                  _log.DebugFormat("TCP TcpServer Listener thread interrupted");
                                                              }
                                                              catch (ThreadAbortException)
                                                              {
                                                                  _log.DebugFormat("TCP TcpServer Listener Thread aborted. Exiting");
                                                              }
                                                              catch (Exception e)
                                                              {
                                                                  _log.ErrorFormat("Exception in TcpServer: {0} {1}",
                                                                                    e.Message, e.StackTrace);
                                                              }
                                                              finally
                                                              {
                                                                  listener.Stop();
                                                              }
                                                          });
            _tpcListenerThread.Start();
        }

        private void RunWorker()
        {

            try
            {
                while (!_stopReading)
                {
                    while (!_stopReading && !_taskResetEvent.WaitOne(200))
                    {

                    }
                    if (_stopReading)
                        break;
                    _taskMutex.WaitOne();
                    TcpClient tcpClient = null;
                    if (_tasks.Count > 0)
                        tcpClient = _tasks.Dequeue();
                    else
                        _taskResetEvent.Reset();
                    _taskMutex.ReleaseMutex();
                    if (tcpClient != null)
                    {
                        ProcessMessage(tcpClient);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("{0} in tcp worker: {1}", ex.GetType().Name, ex.StackTrace);
            }
            _log.Debug("TCP server worker thread exiting");
        }

        /// <summary>
        /// Processes the message.
        /// </summary>
        /// <param name="client">The client.</param>
        private void ProcessMessage(TcpClient client)
        {
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            NetworkStream stream = client.GetStream();
            try
            {
                if (NewMessage == null)
                {
                    _log.DebugFormat("TcpServer: No listeners for new messages found when processing TCP message");
                    return;
                }

                while (!_stopReading)
                {
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
                            int read = stream.Read(buffer, 0, Math.Min(buffer.Length, length - totalbytesRead));
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
                    if (messageType != (short)MessageType.Publish)
                    {
                        break;
                    }

                }
            }
            catch (SocketException e)
            {
                _log.DebugFormat("Caught SocketException, error code {0}", e.ErrorCode);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Exception during processing network message: {0}\n{1}", ex.Message, ex.StackTrace);
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
            _stopReading = true;
            _log.DebugFormat("Stopping server");
            _tpcListenerThread.Interrupt();
            _tpcListenerThread.Join(1000);
            if (_tpcListenerThread.IsAlive)
            {
                _tpcListenerThread.Abort();
            }
            _workerThreads.ForEach(t=>t.Join());
        }
    }
}