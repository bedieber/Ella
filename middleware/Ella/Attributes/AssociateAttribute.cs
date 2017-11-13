//=============================================================================
// Project  : Ella Middleware
// File    : AssociateAttribute.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: https://github.com/bedieber/Ella.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;

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
