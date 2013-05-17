using System;

namespace Ella.Attributes
{
    /// <summary>
    /// This attribute is used to mark a certain method as being a factory to create a new instance of a module
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true)]
    public class FactoryAttribute : Attribute
    {

    }
}
