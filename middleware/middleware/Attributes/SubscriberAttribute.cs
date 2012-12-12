using System;
using Ella.Data;

namespace Ella.Attributes
{
    /// <summary>
    /// This attribute is used to declare a class as a subscriber. This is used upon discovery of subscriber types in an assembly.<br />
    /// If your class is not marked with this attribute, it will not be recognized as a subscriber
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SubscriberAttribute:Attribute
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the modify policy, default is <see cref="DataModifyPolicy.NoModify"/>.
        /// </summary>
        /// <value>
        /// The modify policy to be used when subscribing to an event
        /// </value>
        public DataModifyPolicy ModifyPolicy { get; set; }

        #endregion


    }
}
