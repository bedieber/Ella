using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Ella.Internal
{
    internal class Serializer
    {
        internal static BinaryFormatter _formatter = new BinaryFormatter();

        /// <summary>
        /// Serializes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>A byte[] containing the serialized <paramref name="data"/></returns>
        internal static byte[] Serialize(object data)
        {
            MemoryStream ms = new MemoryStream();
            _formatter.Serialize(ms, data);
            ms.Seek(0, SeekOrigin.Begin);
            return ms.ToArray();

        }

        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>
        /// The deserialized object of type <typeparamref name="T" />
        /// </returns>
        internal static T Deserialize<T>(byte[] data, int offset = 0)
        {
            MemoryStream ms = new MemoryStream(data);
            ms.Seek(offset, SeekOrigin.Begin);
            T result = (T)_formatter.Deserialize(ms);
            return result;
        }

        /// <summary>
        /// Performs copy via serialization.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns>A clone of <paramref name="data"/></returns>
        internal static T SerializeCopy<T>(T data)
        {
            MemoryStream ms = new MemoryStream();
            _formatter.Serialize(ms, data);
            ms.Seek(0, SeekOrigin.Begin);
            T result = (T)_formatter.Deserialize(ms);
            return result;
        }
    }
}
