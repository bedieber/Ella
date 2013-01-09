using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Ella.Model;
using Ella.Network;

namespace Ella.Internal
{
    /// <summary>
    /// Util class for various reflection functions
    /// </summary>
    internal static class ReflectionUtils
    {
        #region private helpers

        /// <summary>
        /// Checks if <paramref name="t"/> defines the attribute <paramref name="attribute"/>.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        internal static bool DefinesAttribute(Type t, Type attribute)
        {
            List<object> atr = new List<object>(t.GetCustomAttributes(attribute, true));
            return atr.Any();
        }

        /// <summary>
        /// Gets the first method defined in <paramref name="type"/> which is attributed with <paramref name="attribute"/> .
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="includeConstructors">if set to <c>true</c> <paramref name="attribute"/> is also searched for in constructors.</param>
        /// <returns>A <see cref="System.Reflection.MethodBase"/> object referring to the first method found or null if no method was found</returns>
        internal static MethodBase GetAttributedMethod(Type type, Type attribute, bool includeConstructors = false)
        {
            IEnumerable<MethodBase> methodInfos = type.GetMethods();
            if (includeConstructors)
            {
                var constructorInfos = type.GetConstructors();
                methodInfos = methodInfos.Concat(constructorInfos);
            }
            foreach (var methodInfo in methodInfos)
            {
                var customAttributes = methodInfo.GetCustomAttributes(attribute, true);
                if (customAttributes.Any())
                    return methodInfo;
            }
            return null;
        }

        /// <summary>
        /// Gets the all methods of the type <paramref name="type"/> which are attributed with <paramref name="attribute"/>
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="includeConstructors">if set to <c>true</c> <paramref name="attribute"/> is also searched for in constructors</param>
        /// <returns></returns>
        internal static IEnumerable<MethodBase> GetAttributedMethods(Type type, Type attribute, bool includeConstructors = false)
        {
            IEnumerable<MethodBase> methodInfos = type.GetMethods();
            if (includeConstructors)
            {
                var constructorInfos = type.GetConstructors();
                methodInfos = methodInfos.Concat(constructorInfos);
            }
            var methods = from m in methodInfos where m.GetCustomAttributes(attribute, true).Any() select m;
            return methods;
        }


        /// <summary>
        /// Searches for any <paramref name="type"/> members attributed with <paramref name="attribute"/>
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        internal static IEnumerable<KeyValuePair<MemberInfo, IEnumerable<Attribute>>> GetAttributedMembers(Type type, Type attribute)
        {
            var memberInfos = type.GetMembers();
            var attributedMembers = from m in memberInfos let atr = m.GetCustomAttributes(attribute, true) where atr.Any() select new KeyValuePair<MemberInfo, IEnumerable<Attribute>>(m, atr.Cast<Attribute>());
            return attributedMembers;
        }

        /// <summary>
        /// Creates a <see cref="Ella.Model.Subscription{T}" /> instance for a specific type.<br />
        /// Since the type parameter might not be known at compile time, we need a method using reflection to create this on the fly.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="match">The match.</param>
        /// <param name="proxy">The proxy.</param>
        /// <returns></returns>
        internal static SubscriptionBase CreateGenericSubscription(Type type, Event match, Proxy proxy)
        {
            Type subscriptionType = typeof(Subscription<>).MakeGenericType(new Type[] { type });
            Type actionType;
            Delegate @delegate;
            if (type.IsValueType)
            {
                actionType = typeof(Action<,>).MakeGenericType(new Type[] { type, typeof(SubscriptionHandle) });

                /* The next few lines create an expression tree to be able to handle value types
                * If e.g. a struct is passed, we cannot directly bind to the proxy.HandleEvent method because the type system refuses to accept an (object) method for a struct
                * Instead we create a lamda expression which perfoms a cast from the value type to object (this can be safely done but only in an explicit manner) and then calls the proxy method
                * The delegate created from this however, is stronly typed and matches the delegate signature needed by the subscription            * 
                * */
                ParameterExpression parameter = Expression.Parameter(type, "data");
                UnaryExpression convertedData = Expression.TypeAs(parameter, typeof(object));
                ParameterExpression handle = Expression.Parameter(typeof(SubscriptionHandle), "handle");
                MethodInfo methodInfo = proxy.GetType().GetMethod("HandleEvent", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodCallExpression e = Expression.Call(Expression.Constant(proxy),
                                                         methodInfo, convertedData, handle);
                LambdaExpression lambda = Expression.Lambda(e, parameter, handle);
                @delegate = lambda.Compile();
            }
            else
            {
                actionType = typeof(Action<,>).MakeGenericType(new Type[] { typeof(object), typeof(SubscriptionHandle) });

                @delegate = Action.CreateDelegate(actionType, proxy, "HandleEvent");
            }
            //Here we call the constructor of our subscription type to generate a new subscription object
            object subscription =
                    subscriptionType.GetConstructor(new Type[] { typeof(object), typeof(Event), actionType })
                                    .Invoke(new object[] { proxy, match, @delegate });
            return (SubscriptionBase)subscription;
        }
        #endregion

    }
}
