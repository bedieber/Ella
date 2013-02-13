using System;
using System.Reflection;
using Ella.Data;

namespace Ella.Model
{
    /// <summary>
    /// Describes one single subscription of one subscriber to one publisher
    /// </summary>
    internal class Subscription : SubscriptionBase
    {
        protected bool Equals(Subscription other)
        {
            return Equals(CallbackMethod, other.CallbackMethod) && Equals(CallbackTarget, other.CallbackTarget)&&Equals(Event, other.Event);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = ((CallbackMethod != null ? CallbackMethod.GetHashCode() : 0) * 397) ^
                           (CallbackTarget != null ? CallbackTarget.GetHashCode() : 0);
                hash = (hash * 397) ^ Event.EventDetail.ID;
                hash = (hash * 397) ^ EllaModel.Instance.GetPublisherId(Event.Publisher);
                return hash;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Subscription" /> class.
        /// </summary>
        public Subscription()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Subscription" /> class.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="ev">The ev.</param>
        /// <param name="callback">The callback.</param>
        public Subscription(object subscriber, Event ev, MethodInfo callbackMethod, object callbackTarget)
            : this()
        {
            Subscriber = subscriber;
            Event = ev;
            CallbackMethod = callbackMethod;
            CallbackTarget = callbackTarget;

            //Callback = callback;
        }

        /// <summary>
        /// Gets or sets the callback which is used to notify a subscriber of a new event.
        /// </summary>
        /// <value>
        /// The callback.
        /// </value>
        //internal Action<T, SubscriptionHandle> Callback { get; set; }

        internal MethodInfo CallbackMethod { get; set; }

        internal object CallbackTarget { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Subscription)obj);
        }
    }
}