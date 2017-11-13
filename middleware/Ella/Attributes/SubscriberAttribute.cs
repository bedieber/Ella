//=============================================================================
// Project  : Ella Middleware
// File    : SubscriberAttribute.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: https://github.com/bedieber/Ella.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using Ella.Data;

namespace Ella.Attributes
{
    /// <summary>
    /// This attribute is used to declare a class as a subscriber. This is used upon discovery of subscriber types in an assembly.<br />
    /// If your class is not marked with this attribute, it will not be recognized as a subscriber
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SubscriberAttribute : Attribute
    {
    }
}
