using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ella.Internal.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    interface ISerialize
    {
        string Description { get; }
        void Initialize();
        byte[] Serialize(object objectToSerialize);

        T Deserialize<T>(byte[] data);
    }
}
