using System;
using Ella.Control;

namespace Ella.Attributes
{
    /// <summary>
    /// This attribut flags a certain method as being used to accept <seealso cref="ApplicationMessage"/>
    /// It must be of the signature <c>void name(ApplicationMessage)</c>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ReceiveMessageAttribute : Attribute
    {
    }
}
