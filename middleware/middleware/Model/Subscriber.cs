using System;
using System.Reflection;

namespace Ella.Model
{
    internal class Subscriber
    {
        internal object Instance { get; set; }
        internal Type SubscriptionType { get; set; }


        internal MethodBase HandlingMethod { get; set; }
        internal MethodBase MessageCallback { get; set; }
        internal MethodBase SubscriptionCallback { get; set; }
    }
}
