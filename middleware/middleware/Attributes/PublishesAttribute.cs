using System;
using Ella.Data;

namespace Ella.Attributes
{
    /// <summary>
    /// Attribute used to declare a publisher
    /// </summary>
    /// <remarks>For each publish event, a publisher must define one Publishes attribute. The <see cref="PublishesAttribute.ID"/> property defines a local ID for this event. No two events of a single publisher may have the same number (however, if two distinct publishers define the same ID, this is allowed)</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PublishesAttribute : Attribute
    {
        #region Private Members
        private Type _dataType;
        private int _id;
        #endregion

        #region Public Properties
        /// <summary>
        /// The data type published by this event
        /// </summary>
        public Type DataType { get { return _dataType; } }
        /// <summary>
        /// The local ID of this publishing event
        /// </summary>
        public int ID { get { return _id; } }

        /// <summary>
        /// Gets or sets the copy policy, default is <see cref="DataCopyPolicy.None"/>.
        /// </summary>
        /// <value>
        /// The copy policy to be used when processing the published data.
        /// </value>
        public DataCopyPolicy CopyPolicy { get; set; }
        #endregion

        /// <summary>
        /// Creates a new PublishesAttribute
        /// </summary>
        /// <param name="dataType">The data type that will be published </param>
        /// <param name="id">The internal ID</param>
        public PublishesAttribute(Type dataType, int id)
        {
            _dataType = dataType;
            _id = id;
            //CopyPolicy = policy;
        }

    }
}
