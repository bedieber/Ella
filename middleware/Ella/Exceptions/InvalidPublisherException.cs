using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ella.Exceptions
{
    /// <summary>
    /// This exception is thrown whenever a publisher module is found to be invalid<br />
    /// Possible reasons for this:
    /// <list type="Bullet">
    /// <item>
    /// No <see cref="Ella.Attributes.PublishesAttribute"/> is defined
    /// </item>
    /// <item>
    /// The publisher defines one event ID multiple times
    /// </item>
    /// <item>
    /// The publisher does not define start and stop methods using <see cref="Ella.Attributes.StartAttribute"/> and <see cref="Ella.Attributes.StopAttribute"/>
    /// </item>
    /// </list>
    /// 
    /// </summary>
    public class InvalidPublisherException : InvalidModuleException
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPublisherException" /> class.
        /// </summary>
        public InvalidPublisherException()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPublisherException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public InvalidPublisherException(string message)
            : base(message)
        {

        }
    }
}
