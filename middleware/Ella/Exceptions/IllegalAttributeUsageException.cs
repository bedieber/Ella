//=============================================================================
// Project  : Ella Middleware
// File    : IllegalAttributeUsageException.cs
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

namespace Ella.Exceptions
{
    /// <summary>
    /// This exception is thrown whenever an Ella attribute is used in an illegal context
    /// </summary>
    public class IllegalAttributeUsageException:Exception
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="IllegalAttributeUsageException"/> class.
        /// </summary>
        public IllegalAttributeUsageException()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IllegalAttributeUsageException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public IllegalAttributeUsageException(string message) : base(message)
        {
            
        }
    }
}
