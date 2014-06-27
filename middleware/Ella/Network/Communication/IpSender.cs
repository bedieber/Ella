//=============================================================================
// Project  : Ella Middleware
// File    : Sender.cs
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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;

namespace Ella.Network.Communication
{
    /// <summary>
    /// The networking client used to contact a remote endpoint
    /// </summary>
    internal partial class IpSender : SenderBase, IDisposable
    {
        private string _address;
        private int _port;


        private static readonly ILog _log = LogManager.GetLogger(typeof(IpSender));

        private bool _run;
        private int _maxQueueSize = Ella.Internal.EllaConfiguration.Instance.MaxQueueSize;

        private readonly Queue<Message> _pendingMessages = new Queue<Message>();
        private readonly AutoResetEvent _are = new AutoResetEvent(false);
        private Thread _senderThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="IpSender"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        public IpSender(string address, int port)
        {
            this._address = address;
            this._port = port;
        }

        /// <summary>
        /// Gets or sets the maximal size of the queue.
        /// </summary>
        /// <value>
        /// The maximal size of the queue.
        /// </value>
        public int MaxQueueSize
        {
            get { return _maxQueueSize; }
            set { _maxQueueSize = value; }
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="m">The message</param>
        /// <returns><c>true </c> If Queue was cleared due to exceeding the buffer size.<c>false</c> otherwise</returns>
        internal bool EnqueueMessage(Message m)
        {
            bool queueCleared = false;
            if (_senderThread == null)
            {
                _run = true;
                _log.Debug("Starting sender thread");
                _senderThread = new Thread(Run);
                _senderThread.Start();
            }
            if (_pendingMessages.Count > MaxQueueSize)
            {
                _log.Debug("Too many items in queue. Clearing");
                _pendingMessages.Clear();
                queueCleared = true;
            }
            _pendingMessages.Enqueue(m);
            _log.DebugFormat("Enqueued message, {0} items in queue", _pendingMessages.Count);
            _are.Set();
            return queueCleared;
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="m">The message to send</param>
        internal override void Send(Message m)
        {
            try
            {
                TcpClient client = new TcpClient();
                IAsyncResult ar = client.BeginConnect(IPAddress.Parse(_address), _port, null, null);

                if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false))
                {
                    client.Close();
                    _log.WarnFormat("Could not connect to {0} in time, aborting send operation", _address);
                    return;
                }

                client.EndConnect(ar);
                //GZipStream stream = new GZipStream(client.GetStream(), CompressionMode.Compress);
                NetworkStream stream = client.GetStream();
                byte[] serialize = m.Serialize();
                stream.Write(serialize, 0, serialize.Length);
                stream.Flush();
                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                _log.WarnFormat("NetworkClient: failed to send message {0} to {1}", m.Id,
                                  _address, e.Message);
            }
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        private void Run()
        {
            while (_run)
            {
                _log.Debug("IpSender running, connecting to remote host");
                TcpClient client = new TcpClient();
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                IAsyncResult ar = client.BeginConnect(IPAddress.Parse(_address), _port, null, null);

                if (!_run || !ar.AsyncWaitHandle.WaitOne(5000))
                {
                    client.Close();
                    _log.WarnFormat("Could not connect to {0} in time, aborting send operation", _address);
                    return;
                }

                try
                {
                    client.EndConnect(ar);
                }
                catch (SocketException socketEx)
                {
                    _log.ErrorFormat("Initial connect to host {0} failed", _address);
                }
                if (client.Connected)
                {

                    NetworkStream stream = client.GetStream();
                    _log.DebugFormat("IpSender connected to {0}:{1}", _address, _port);
                    try
                    {
                        while (_run && client.Connected)
                        {
                            while (_pendingMessages.Count == 0 && _are.Reset() && _are.WaitOne(2000))
                            {
                                _log.Debug("waiting for new messages");
                            }
                            Message m = _pendingMessages.Dequeue();
                            byte[] serialize = m.Serialize();

                            _log.DebugFormat("Transferring {0} bytes of data", serialize.Length);

                            stream.Write(serialize, 0, serialize.Length);
                            stream.Flush();
                        }
                    }
                    catch (SocketException sex)
                    {
                        _log.WarnFormat("NetworkClient: failed to send message  {0} with socket error: {1}",
                                          _address, sex.ErrorCode);
                    }
                    catch (Exception e)
                    {
                        _log.WarnFormat("NetworkClient: failed to send message to {0}: {1} \n{2}",
                                          _address, e.Message, e);
                    }
                    finally
                    {
                        stream.Close();
                        client.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _log.Debug("Disposing IpSender");
            _run = false;
            _senderThread.Interrupt();
        }
    }
}