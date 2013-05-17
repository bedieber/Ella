using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ella.Data
{
    /// <summary>
    /// The base class of transfer objects used to pass data between modules
    /// </summary>
    [Serializable]
    public abstract class DataTransferObject
    {
        /// <summary>
        /// Gets the timestamp.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        public DateTime Timestamp
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the publisher of this data object.
        /// </summary>
        /// <value>
        /// The publisher.
        /// </value>
        public SubscriptionHandle Handle { get; internal set; }

    }
}
