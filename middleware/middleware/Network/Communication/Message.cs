using System;
using System.IO;
using System.Text;
using System.Threading;
using Ella.Internal;

namespace Ella.Network.Communication
{
    /// <summary>
    /// Defines the type of message contained
    /// </summary>
    internal enum MessageType : byte
    {
        Discover = 0,
        Subscribe = 1,
        SubscribeResponse,
        RequestTemplate,
        Publish,
        DiscoverResponse
    }
    
    

    /// <summary>
    /// A class to encapsulate a single network message
    /// </summary>
    internal class Message
    {
        private static int _nextId = new Random().Next(100);
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public MessageType Type { get; set; }
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public byte[] Data { get; set; }
        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public int Id { get; private set; }
        /// <summary>
        /// Gets or sets the sender.
        /// </summary>
        /// <value>
        /// The sender.
        /// </value>
        public int Sender { get; set; }

        /// <summary>
        /// Gets the next id.
        /// </summary>
        /// <value>
        /// The next id.
        /// </value>
        public static int NextId
        {
            get { return _nextId; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Message" /> class.
        /// </summary>
        public Message()
        {
            Id = Interlocked.Increment(ref _nextId);
            Sender = EllaConfiguration.Instance.NodeId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message" /> class.
        /// </summary>
        /// <param name="id">The id.</param>
        public Message(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Serializes this message.
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte((byte)Type);
            byte[] bytes = BitConverter.GetBytes(Id);
            ms.Write(bytes, 0, bytes.Length);
            bytes = BitConverter.GetBytes(Sender);
            ms.Write(bytes, 0, bytes.Length);
            bytes = BitConverter.GetBytes(Data.Length);
            ms.Write(bytes, 0, bytes.Length);
            ms.Write(Data, 0, Data.Length);
            return ms.ToArray();
        }

    }
}