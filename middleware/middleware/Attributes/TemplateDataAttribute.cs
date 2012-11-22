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
