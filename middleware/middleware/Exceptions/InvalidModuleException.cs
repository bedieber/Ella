using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ella.Exceptions
{
    /// <summary>
    /// This exception indicates that a specific type is neither publisher nor subscriber
    /// </summary>
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
