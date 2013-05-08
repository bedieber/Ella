using System.Collections.Generic;
using System.Reflection;
using Ella.Control;

namespace Ella.Model
{

    internal class Publisher
    {
        //TODO maybe add id and remove from EllaModel
        protected bool Equals(Publisher other)
        {
            return Equals(Instance, other.Instance);
        }

        public override int GetHashCode()
        {
            return (Instance != null ? Instance.GetHashCode() : 0);
        }

        internal object Instance { get; set; }

        internal IEnumerable<Event> Events { get; set; }

        //TODO validate methodinfos on set
        //Maybe use extension methods for messageinfo or validators
        internal MethodBase MessageCallback { get; set; }

        internal MethodBase StartMethod { get; set; }

        internal MethodBase StopMethod { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Publisher) obj);
        }
    }
}
