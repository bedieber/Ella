//=============================================================================
// Project  : Ella Middleware
// File    : Create.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@uni-klu.ac.at)
// Copyright 2012 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ella.Attributes;
using Ella.Exceptions;
using Ella.Internal;
using log4net;

namespace Ella
{
    /// <summary>
    /// This class is used for creating module instances and template objects
    /// </summary>
    public static class Create
    {
        private static ILog _log = LogManager.GetLogger(typeof(Create));
        /// <summary>
        /// Creates an instance of a module type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Class does not define static factory method defining the [Factory] Attribute</exception>
        public static object ModuleInstance(Type type)
        {
            _log.DebugFormat("Creating instance of {0}", type);
            if (Is.Publisher(type) || Is.Subscriber(type))
            {
                var methodInfo = ReflectionUtils.GetAttributedMethod(type, typeof(FactoryAttribute), true);
                if (methodInfo != null && !methodInfo.GetParameters().Any() &&
                    (methodInfo is ConstructorInfo || (methodInfo is MethodInfo && methodInfo.IsStatic)))
                {
                    object instance = null;
                    if (methodInfo is ConstructorInfo)
                        instance = (methodInfo as ConstructorInfo).Invoke(null);
                    else
                        instance = methodInfo.Invoke(new object[] { }, null);
                    return instance;
                }
                else
                {
                    _log.ErrorFormat("{0} does not define static factory method defining the [Factory] Attribute", type);
                    throw new ArgumentException(
                        "Class does not define static factory method defining the [Factory] Attribute");
                }
            }
            else
            {
                _log.ErrorFormat("{0} is no valid publisher or subscriber", type);
                throw new InvalidModuleException(string.Format("{0} is no valid pubisher or subscriber", type));
            }
        }

        /// <summary>
        /// Gets a template object from the publisher instance using the specified event ID<br />
        /// A template data providing member (method or propery) may take an eventID as parameter
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="eventId">The event id.</param>
        /// <returns></returns>
        /// <exception cref="InvalidPublisherException">TemplateData Method may require at most one parameter</exception>
        public static object TemplateObject(object instance, int eventId)
        {
            _log.DebugFormat("Creating template object of event {0} defined in type {1} using instance {2}", eventId,
                             instance.GetType(), instance);
            if (Is.ValidPublisher(instance.GetType()))
            {
                /*
                 * OK -- Resolve the method or property attributed with TemplateDataAttribute
                 * OK -- Return value must be of the same type as the published event
                 * OK -- methods may have one parameter which is the integer eventID
                 * OK -- the eventId supplied in the TemplateDataAttribute must match
                 * 
                 */
                var members = ReflectionUtils.GetAttributedMembers(instance.GetType(), typeof(TemplateDataAttribute));
                foreach (var m in members)
                {
                    IEnumerable<bool> attributes = m.Value.Select(a => (a as TemplateDataAttribute).EventID == eventId);
                    if (attributes.Any())
                    {

                        Type targetType =
                            (instance.GetType().GetCustomAttributes(typeof(PublishesAttribute), true)).Where(
                                a => (a as PublishesAttribute).ID == eventId).Cast<PublishesAttribute>().First().
                                DataType;
                        //Check the type of member (property or method)
                        //get the result
                        object templateObject = null;
                        if (m.Key is MethodInfo)
                        {
                            var methodInfo = m.Key as MethodInfo;
                            var parameters = methodInfo.GetParameters();
                            if (parameters.Count() > 1)
                            {
                                _log.ErrorFormat(
                                    "{0} is not well defined for being a template method. It may require at most one parameter, which is the Event ID (int)",
                                    m.Key);
                                throw new InvalidPublisherException("TemplateData Method may require at most one parameter");
                            }
                            if (parameters.Count() == 1)
                                templateObject = methodInfo.Invoke(instance, new object[] { eventId });
                            else
                            {
                                templateObject = methodInfo.Invoke(instance, null);
                            }
                        }
                        else
                        {
                            var propertyInfo = m.Key as PropertyInfo;
                            ParameterInfo[] parameters = propertyInfo.GetIndexParameters();
                            if (parameters.Count() > 1)
                            {
                                _log.ErrorFormat(
                                    "{0} is not well defined for being a template method. It may require at most one parameter, which is the Event ID (int)",
                                    m.Key);
                                throw new InvalidPublisherException("TemplateData Property may require at most one index variable");
                            }
                            if (parameters.Count() == 1)
                                templateObject = propertyInfo.GetValue(instance, new object[] { eventId });
                            else
                            {
                                templateObject = propertyInfo.GetValue(instance, null);
                            }
                        }
                        //check if the template object is of the same type as defined in the PublishesAttribute
                        if (targetType != templateObject.GetType())
                        {
                            _log.ErrorFormat("Template object {0} is not of type {1}", templateObject.GetType(),
                                             targetType);
                            throw new InvalidOperationException("Template objects was not of the same type as defined in the PublishesAttribute");
                        }
                        return templateObject;
                    }
                    else
                        _log.WarnFormat("{0} does not provide any template objects for event ID {1}", instance.GetType(),
                                        eventId);
                }
                
            }
            else
            {
                _log.ErrorFormat("{0} is not a valid publisher", instance.GetType().ToString());
                throw new InvalidPublisherException(instance.GetType().ToString());
            }
            return null;
        }
    }
}
