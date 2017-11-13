using Ella.Internal.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ella.Attributes
{
    /// <summary>
    /// This attribute is used to indicate the serialization protocol implemented by a certain <seealso cref="ISerialize"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SerializationProtocolAttribute:Attribute
    {
        private string _protocolDescription;
        /// <summary>
        /// A string value describing the serialization protocol implemented by this type
        /// </summary>
        public string ProtocolDescription { get { return _protocolDescription; } set { _protocolDescription = value; } }
        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="protocolDescription"></param>
        public SerializationProtocolAttribute(string protocolDescription)
        {
            _protocolDescription = protocolDescription;
        }
    }
}
