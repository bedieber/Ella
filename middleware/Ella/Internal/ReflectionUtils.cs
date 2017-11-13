//=============================================================================
// Project  : Ella Middleware
// File    : ReflectionUtils.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: https://github.com/bedieber/Ella.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Ella.Attributes;
using Ella.Exceptions;
using Ella.Model;
using Ella.Network;
using log4net;

namespace Ella.Internal
{
    /// <summary>
    /// Util class for various reflection functions
    /// </summary>
    internal static class ReflectionUtils
    {
        private static ILog _log = LogManager.GetLogger(typeof(ReflectionUtils));
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
        #endregion

        #region publisher handling

        /// <summary>
        /// Creates the publisher.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        /// <exception cref="InvalidPublisherException">
        /// Publisher does not define a start method
        /// or
        /// No valid stop method found
        /// or
        /// </exception>
        internal static Publisher CreatePublisher(object instance)
        {
            var type = instance.GetType();
            if (Is.ValidPublisher(type))
            {
                var startMethod = ReflectionUtils.GetAttributedMethod(type, typeof (StartAttribute));
                if (startMethod == null)
                {
                    _log.ErrorFormat("{0} does not define a start method", instance.GetType());
                    throw new InvalidPublisherException("Publisher does not define a start method");
                }
                if (!startMethod.GetParameters().Any())
                {
                    var stopMethod = ReflectionUtils.GetAttributedMethod(type, typeof (StopAttribute));

                    if (stopMethod == null)
                    {
                        _log.ErrorFormat("{0} does not define a stop method", instance.GetType());
                        throw new InvalidPublisherException("No valid stop method found");
                    }

                    if (!stopMethod.GetParameters().Any())
                    {
                        var events =
                            instance.GetType()
                                    .GetCustomAttributes(typeof (PublishesAttribute), false)
                                    .Select(a => new Event {Publisher = instance, EventDetail = a as PublishesAttribute});
                        Publisher p = new Publisher
                            {
                                Instance = instance,
                                StartMethod = startMethod,
                                StopMethod = stopMethod,
                                Events = events
                            };
                        return p;
                    }
                }
            }
            else
            {
                _log.ErrorFormat("{0} is not a valid publisher", instance.GetType());
                throw new InvalidPublisherException(instance.GetType().ToString());
            }
            return null;
        }

        #endregion
    }
}
