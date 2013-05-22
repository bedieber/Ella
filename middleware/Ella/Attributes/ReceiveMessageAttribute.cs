//=============================================================================
// Project  : Ella Middleware
// File    : ReceiveMessageAttribute.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@uni-klu.ac.at)
// Copyright 2012 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

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
