using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ella.Attributes
{
    /// <summary>
    /// This attribute is used to mark methods which can process event associations<br />
    /// A publisher can associate two events to indicate some kind of relation between them
    /// </summary>
    /// <remarks><see cref="AssociateAttribute"/> can only be used in subscriber classes and needs the signature <c>void(SubscriptionHandle, SubscriptionHandle)</c><br /></remarks>
    /// 
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class AssociateAttribute : Attribute
    {

    }
}
