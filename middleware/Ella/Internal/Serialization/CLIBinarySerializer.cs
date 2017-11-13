using Ella.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Ella.Internal.Serialization
{
    [SerializationProtocol("CLI-Binary")]
    internal class CLIBinarySerializer : ISerialize
    {

        BinaryFormatter _formatter = new BinaryFormatter();

        public T Deserialize<T>(byte[] data, int offset = 0)
        {
            MemoryStream ms = new MemoryStream(data);
            ms.Seek(offset, SeekOrigin.Begin);
            T result = (T)_formatter.Deserialize(ms);
            return result;
        }

        public void Initialize()
        {
            
        }

        public byte[] Serialize(object objectToSerialize)
        {
            MemoryStream ms = new MemoryStream();
            _formatter.Serialize(ms, objectToSerialize);
            ms.Seek(0, SeekOrigin.Begin);
            return ms.ToArray();
        }
    }
}
