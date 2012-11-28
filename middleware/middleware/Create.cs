using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ella.Attributes;
using Ella.Exceptions;
using Ella.Internal;

namespace Ella
{
    /// <summary>
    /// This class is used for creating module instances and template objects
    /// </summary>
    public static class Create
    {
        /// <summary>
        /// Creates an instance of a module type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Class does not define static factory method defining the [Factory] Attribute</exception>
        public static object ModuleInstance(Type type)
        {
            if (Is.Publisher(type) || Is.Subscriber(type))
            {
                var methodInfo = ReflectionUtils.GetAttributedMethod(type, typeof(FactoryAttribute), true);
                if (methodInfo != null && !methodInfo.GetParameters().Any() && (methodInfo is ConstructorInfo || (methodInfo is MethodInfo && methodInfo.IsStatic)))
                {
                    object instance = null;
                    if (methodInfo is ConstructorInfo)
                        instance = (methodInfo as ConstructorInfo).Invoke(null);
                    else
                        instance = methodInfo.Invoke(new object[] { }, null);
                    return instance;
                }
            }
            throw new ArgumentException("Class does not define static factory method defining the [Factory] Attribute");
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
            if (Is.ValidPublisher(instance.GetType()))
            {
                /*
                 * OK -- Resolve the method or property attributed with TemplateDataAttribute
                 * Return value must be of the same type as the published event
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
                                throw new InvalidPublisherException("TemplateData Method may require at most one parameter");
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
                                throw new InvalidPublisherException("TemplateData Property may require at most one index variable");
                            if (parameters.Count() == 1)
                                templateObject = propertyInfo.GetValue(instance, new object[] { eventId });
                            else
                            {
                                templateObject = propertyInfo.GetValue(instance, null);
                            }
                        }
                        //check if the template object is of the same type as defined in the PublishesAttribute
                        if (targetType != templateObject.GetType())
                            throw new InvalidOperationException("Template objects was not of the same type as defined in the PublishesAttribute");
                        return templateObject;
                    }
                }
            }
            else
            {
                throw new InvalidPublisherException(instance.GetType().ToString());
            }
            return null;
        }
    }
}
