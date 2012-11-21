using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ella.Attributes
{
    /// <summary>
    /// This attribute marks a parameterless method as being the entry point for a publisher module<br />
    /// </summary>
    /// <remarks>
    /// The start method will be called in a dedicated thread in order to start a certain publisher module.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class StartAttribute:Attribute
    {
    }
}
