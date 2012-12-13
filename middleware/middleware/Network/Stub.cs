using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Ella.Attributes;
using Ella.Data;
using Ella.Network.Communication;

namespace Ella.Network
{
    /// <summary>
    /// A stub acts as the local representative of a remote publisher
    /// </summary>
    [Publishes(typeof(Unknown), 1, CopyPolicy = DataCopyPolicy.None)]
    internal class Stub<T>
    {
        
        [Start]
        public void Start()
        {

        }

        [Stop]
        public void Stop()
        {

        }

        [Factory]
        public Stub<T> CreateInstance()
        {
            return new Stub<T>();
        }

        internal void NewMessage(byte[] data)
        {
            BinaryFormatter bf=new BinaryFormatter();
            T dto=(T)bf.Deserialize(new MemoryStream(data));
            Publish.Event(dto,this,1);
        }
    }
}
