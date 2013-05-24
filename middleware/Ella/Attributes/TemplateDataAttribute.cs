//=============================================================================
// Project  : Ella Middleware
// File    : TemplateDataAttribute.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ella.Attributes
{
    /// <summary>
    /// This attribute signals that the attributed method or property returns a template object for a specified event
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class TemplateDataAttribute : Attribute
    {
        private int _eventId;

        /// <summary>
        /// Gets the event ID.
        /// </summary>
        /// <value>
        /// The event ID.
        /// </value>
        public int EventID
        {
            get { return _eventId; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateDataAttribute" /> class.
        /// </summary>
        /// <param name="id">The id.</param>
        public TemplateDataAttribute(int id)
        {
            _eventId = id;
        }
    }
}
