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
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using Ella.Internal;
using log4net;

namespace Ella.Network.Communication
{

    /// <summary>
    /// The networking client used to contact a remote endpoint
    /// </summary>
    internal partial class Sender : IDisposable
    {
        private string _address;
        private int _port;

        private bool _run;
        private int _maxQueueSize = int.MaxValue;

        private readonly Queue<Message> _pendingMessages = new Queue<Message>();
        private readonly AutoResetEvent _are = new AutoResetEvent(false);
        private Thread _senderThread;

        public Sender(string address, int port)
        {
            this._address = address;
            this._port = port;
        }

        public int MaxQueueSize
        {
            get { return _maxQueueSize; }
            set { _maxQueueSize = value; }
        }

        internal void Send(Message m)
        {
            if (_senderThread == null)
            {
                _run = true;
                _log.Debug("Starting seder thread");
                _senderThread = new Thread(Run);
                _senderThread.Start();
            }
            if (_pendingMessages.Count > MaxQueueSize)
            {
                _log.Debug("Too many items in queue. Clearing");
                _pendingMessages.Clear();
            }
            _pendingMessages.Enqueue(m);
            _log.DebugFormat("Enqueued message, {0} items in queue", _pendingMessages.Count);
            _are.Set();
        }

        private void Run()
        {
            _log.Debug("Sender running, connecting to remote host");
            TcpClient client = new TcpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            IAsyncResult ar = client.BeginConnect(IPAddress.Parse(_address), _port, null, null);

            if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false))
            {
                client.Close();
                _log.WarnFormat("Could not connect to {0} in time, aborting send operation", _address);
                return;
            }

            client.EndConnect(ar);
            GZipStream stream = new GZipStream(client.GetStream(), CompressionMode.Compress);
            _log.DebugFormat("Sender connected to {0}:{1}", _address, _port);
            try
            {

                while (_run)
                {
                    if (_pendingMessages.Count == 0)
                    {
                        _are.Reset();
                        _log.Debug("waiting for new messages");
                        _are.WaitOne();
                    }
                    Message m = _pendingMessages.Dequeue();
                    byte[] serialize = m.Serialize();
                    _log.DebugFormat("Transferring {0} bytes of data", serialize.Length);
                    //_client.GetStream().Write(serialize, 0, serialize.Length);
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

        public void Dispose()
        {
            _log.Debug("Disposing Sender");
            _run = false;
            _senderThread.Interrupt();
        }
    }
}