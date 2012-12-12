using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ella.Attributes;
using Ella.Data;

namespace Ella.Network
{
    [Publishes(typeof (Unknown), 1,CopyPolicy = DataCopyPolicy.None)]
    internal class Stub
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
        public Stub CreateInstance()
        {
            return new Stub();
        }
    }
}
