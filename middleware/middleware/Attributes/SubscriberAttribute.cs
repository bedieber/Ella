using System;

namespace Ella.Attributes
{
    /// <summary>
    /// This attribute is used to declare a class as a subscriber. This is used upon discovery of subscriber types in an assembly.<br />
    /// If your class is not marked with this attribute, it will not be recognized as a subscriber
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SubscriberAttribute:Attribute
    {
    }
}
