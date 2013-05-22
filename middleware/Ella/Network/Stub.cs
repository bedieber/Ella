//=============================================================================
// Project  : Ella Middleware
// File    : Stub.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@uni-klu.ac.at)
// Copyright 2012 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Ella.Attributes;
using Ella.Data;
using Ella.Internal;
using Ella.Network.Communication;
using log4net;

namespace Ella.Network
{
    internal abstract class Stub
    {
        internal Type DataType { get; set; }
        internal SubscriptionHandle Handle { get; set; }

        /// <summary>
        /// Handles a new message containing a published event from a remote host
        /// </summary>
        /// <param name="data">The data.</param>
        internal abstract void NewMessage(byte[] data);
    }

    /// <summary>
    /// A stub acts as the local representative of a remote publisher
    /// </summary>
    [Publishes(typeof(Unknown), 1, CopyPolicy = DataCopyPolicy.None)]
    internal class Stub<T> : Stub
    {
        private ILog _log = LogManager.GetLogger(typeof(Stub));

        /// <summary>
        /// Starts this instance.
        /// </summary>
        [Start]
        public void Start()
        {

        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        [Stop]
        public void Stop()
        {

        }

        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <returns></returns>
        [Factory]
        public Stub<T> CreateInstance()
        {
            return new Stub<T>();
        }

        /// <summary>
        /// Handles a new message containing a published event from a remote host
        /// </summary>
        /// <param name="data">The data.</param>
        internal override void NewMessage(byte[] data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            var dto = bf.Deserialize(new MemoryStream(data));
            if (dto.GetType() == typeof (T))
            {
                T d = (T)dto;
                Publish.Event(d, this, 1);
            }
        }
    }
}
