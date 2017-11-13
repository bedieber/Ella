using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ella.Internal.Serialization
{
    /// <summary>
    /// Interface for serializers
    /// </summary>
    interface ISerialize
    {
        /// <summary>
        /// Initialization steps are performed here
        /// </summary>
        void Initialize();
        /// <summary>
        /// Serializes the <paramref name="objectToSerialize"/> to the returned byte array.
        /// </summary>
        /// <param name="objectToSerialize">The object which should be serialized</param>
        /// <returns>A binary stream of the serialized data</returns>
        byte[] Serialize(object objectToSerialize);
        /// <summary>
        /// Deserializes a binary data object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        T Deserialize<T>(byte[] data, int offset=0);
    }
}
