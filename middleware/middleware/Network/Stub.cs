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
            _log.DebugFormat("Processing event of {0} bytes", data.Length);
            BinaryFormatter bf = new BinaryFormatter();
            var dto = bf.Deserialize(new MemoryStream(data));
            if (dto.GetType() == typeof (T))
            {
                T d = (T)dto;
                Publish.Event(d, this, 1);
            }
            //TODO log any irregularities
        }
    }
}
