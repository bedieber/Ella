using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Middleware.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true)]
    public class FactoryAttribute : Attribute
    {

    }
}
