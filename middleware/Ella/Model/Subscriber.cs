using System;
using System.Reflection;
using Ella.Data;

namespace Ella.Model
{
    internal class SubscriptionRequest
    {
        internal object SubscriberInstance { get; set; }
        internal Action SubscriptionCall { get; set; }
        internal Type RequestedType { get; set; }
    }
}
