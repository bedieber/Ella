//=============================================================================
// Project  : Ella Middleware
// File    : StateException.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@uni-klu.ac.at)
// Copyright 2012 by Bernhard Dieber, Jennifer Simonjan
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
    /// This exception is thrown whenever an operation is requested which is not allowed in the current state of the application
    /// </summary>
    public class StateException : Exception
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="StateException" /> class.
        /// </summary>
        public StateException() : base()
        {
            
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="StateException" /> class.
        /// </summary>
        /// <param name="message">The exception message</param>
        public StateException(string message):base(message)
        {

        }
    }
}
