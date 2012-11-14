using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Middleware.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PublisherAttribute:Attribute
    {
        #region Private Members
        private Type _dataType;
        private int _id;
        #endregion

        #region Public Properties
        public Type DataType { get { return _dataType; } }
        public int ID { get { return _id; } }
        #endregion

        public PublisherAttribute(Type dataType, int id)
        {
            _dataType = dataType;
            _id = id;
        }
        
    }
}
