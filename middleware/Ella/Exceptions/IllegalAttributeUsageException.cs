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
