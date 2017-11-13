//=============================================================================
// Project  : Ella Middleware
// File    : InvalidModuleException.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: https://github.com/bedieber/Ella.   This notice, the
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
    /// This exception indicates that a specific type is neither publisher nor subscriber
    /// </summary>
    [Serializable]
    public class InvalidModuleException:Exception
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidModuleException" /> class.
        /// </summary>
        public InvalidModuleException()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidModuleException" /> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public InvalidModuleException(string message) : base(message)
        {
            
        }
    }
}
