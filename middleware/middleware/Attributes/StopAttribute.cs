using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ella.Attributes
{
    /// <summary>
    /// This attribute marks a parameterless method as being the exit point for a publisher module<br />
    /// </summary>
    /// <remarks>
    /// The stop method will be called by a dedicated thread in order to stop a certain publisher module.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class StopAttribute : Attribute
    {
    }
}
