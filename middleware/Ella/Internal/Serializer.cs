//=============================================================================
// Project  : Ella Middleware
// File    : SerializationHelper.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: https://github.com/bedieber/Ella.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using Ella.Attributes;
using Ella.Internal.Serialization;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Ella.Internal
{
    internal class SerializationHelper
    {
        //TODO 
        // Assign weights to serializers
        // Extend handshake protocol to include agreement on serialization mechanism

        private static ILog _log = LogManager.GetLogger(typeof(Stop));

        static SerializationHelper()
        {
            Load.Serializers(typeof(SerializationHelper).Assembly);
        }

        /// <summary>
        /// Serializes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="serializerProtocol">An optional parameter to select a different serializer implementation</param>
        /// <returns>A byte[] containing the serialized <paramref name="data"/></returns>
        internal static byte[] Serialize(object data, string serializerProtocol = null)
        {
            if (serializerProtocol == null)
                serializerProtocol = "CLI-Binary";
            var serializer = GetSerializer(serializerProtocol);
            if (serializer == null)
                return null;
            return serializer.Serialize(data);
        }

        /// <summary>
        /// Deserializes the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="serializerProtocol">Optionally select a different serializer implementation</param>
        /// <returns>
        /// The deserialized object of type <typeparamref name="T" />
        /// </returns>
        internal static T Deserialize<T>(byte[] data, int offset = 0, string serializerProtocol = null)
        {
            if (serializerProtocol == null)
                serializerProtocol = "CLI-Binary";
            var serializer = GetSerializer(serializerProtocol);
            if (serializer == null)
                return default(T);
            return serializer.Deserialize<T>(data, offset);
        }

        /// <summary>
        /// Performs copy via serialization.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns>A clone of <paramref name="data"/></returns>
        internal static T SerializeCopy<T>(T data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, data);
            ms.Seek(0, SeekOrigin.Begin);
            T result = (T)formatter.Deserialize(ms);
            return result;
        }

        /// <summary>
        /// Loads a serializer from a given type
        /// </summary>
        /// <returns></returns>
        internal static ISerialize GetSerializer(string protocolDescription)
        {
            try
            {
                var serializerType = Model.EllaModel.Instance.Serializers.Where(t => t.GetCustomAttributes(typeof(SerializationProtocolAttribute), false).Any(a => (a as SerializationProtocolAttribute).ProtocolDescription.Equals(protocolDescription, StringComparison.InvariantCultureIgnoreCase))).FirstOrDefault();
                if (serializerType == null)
                    return null;
                var serializer = (ISerialize)serializerType.Assembly.CreateInstance(serializerType.FullName);
                serializer.Initialize();
                return serializer;
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in Serializer: {0} - {1} ", ex.Message, ex.GetType().FullName);
                return null;
            }
        }
    }
}
