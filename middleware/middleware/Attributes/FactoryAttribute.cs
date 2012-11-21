using System;

namespace Ella.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true)]
    public class FactoryAttribute : Attribute
    {

    }
}
