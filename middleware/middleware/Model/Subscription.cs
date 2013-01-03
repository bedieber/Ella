using System;
using Ella.Data;

namespace Ella.Model
{
    /// <summary>
    /// Describes one single subscription of one subscriber to one publisher
    /// </summary>
    internal class Subscription<T> : SubscriptionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Subscription{T}" /> class.
        /// </summary>
        public Subscription()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Subscription{T}" /> class.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="ev">The ev.</param>
        /// <param name="callback">The callback.</param>
        public Subscription(object subscriber, Event ev, Action<T> callback)
            : this()
        {
            Subscriber = subscriber;
            Event = ev;
            Callback = callback;
        }

        /// <summary>
        /// Gets or sets the callback which is used to notify a subscriber of a new event.
        /// </summary>
        /// <value>
        /// The callback.
        /// </value>
        internal Action<T> Callback { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Subscription<T>)obj);
        }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        protected bool Equals(Subscription<T> other)
        {
            return Equals(Subscriber, other.Subscriber) && Equals(Event, other.Event) && Equals(Callback, other.Callback);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Subscriber != null ? Subscriber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Event != null ? Event.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Callback != null ? Callback.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}