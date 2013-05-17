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
