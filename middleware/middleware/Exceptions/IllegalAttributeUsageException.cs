using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ella.Exceptions
{
    public class IllegalAttributeUsageException:Exception
    {
        public IllegalAttributeUsageException()
        {
            
        }

        public IllegalAttributeUsageException(string message) : base(message)
        {
            
        }
    }
}
