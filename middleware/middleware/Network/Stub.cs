using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Ella.Attributes;
using Ella.Data;
using Ella.Internal;
using Ella.Network.Communication;

namespace Ella.Network
{
    /// <summary>
    /// A stub acts as the local representative of a remote publisher
    /// </summary>
    [Publishes(typeof(Unknown), 1, CopyPolicy = DataCopyPolicy.None)]
    internal class Stub
    {

        internal Type DataType { get; set; }

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
        public Stub CreateInstance()
        {
            return new Stub();
        }

        internal SubscriptionHandle Handle { get; set; }

        /// <summary>
        /// Handles a new message containing a published event from a remote host
        /// </summary>
        /// <param name="data">The data.</param>
        internal void NewMessage(byte[] data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            var dto = bf.Deserialize(new MemoryStream(data));
            if (dto.GetType() == this.DataType)
                Publish.Event(dto, this, 1);
            //TODO log any irregularities
        }
    }
}
