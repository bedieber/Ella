using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Middleware.Attributes
{
    /// <summary>
    /// Attribute used to declare a publisher
    /// </summary>
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
        }

    }
}
