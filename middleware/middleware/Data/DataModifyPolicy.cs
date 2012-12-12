using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ella.Data
{
    /// <summary>
    /// This attribute is used by subscribers to indicate whether they modify the published data or not
    /// </summary>
    public enum DataModifyPolicy
    {
        /// <summary>
        /// Data is not modified by the subscriber <br />
        /// </summary>
        NoModify,
        /// <summary>
        /// The data will be modified by the subscriber <br />
        /// </summary>
        Modify
    }
}
