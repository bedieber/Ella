using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Ella.Attributes;

namespace Ella
{
    /// <summary>
    /// <remarks>
    /// Name to be refactored
    /// </remarks>
    /// </summary>
    public class Middleware
    {
        /// <summary>
        /// Constructor for the middleware
        /// </summary>
        public Middleware()
        {
            Publishers = new List<Type>();
            Subscribers = new List<Type>();
        }

        /// <summary>
        /// List of all known publishers
        /// </summary>
        public ICollection<Type> Publishers
        {
            get;
            set;
        }

        /// <summary>
        /// List of all known subscriber types
        /// </summary>
        public ICollection<Type> Subscribers { get; set; }

        #region public methods
        /// <summary>
        /// Loads all publishers from a given assembly
        /// </summary>
        /// <param name="a">The assembly where to search publishers in</param>
        public void LoadPublishers(Assembly a)
        {

            if (a == (Assembly)null)
                throw new ArgumentNullException("a");
            //AssemblyName[] referencedAssemblies = a.GetReferencedAssemblies();
            //foreach (AssemblyName name in referencedAssemblies)
            //{
            //    Assembly.Load(name);
            //}
            Type[] exportedTypes = a.GetExportedTypes();
            foreach (Type t in exportedTypes)
            {
                if (IsPublisher(t))
                {
                    if (ArePublisherEventsValid(t))
                        Publishers.Add(t);
                }
            }
        }
        /// <summary>
        /// Load all Subscribers from a given assembly<br />
        /// <remarks>Any type must define the <see cref="Ella.Attributes.SubscriberAttribute"/> attribute in order to be detected as subcriber</remarks>
        /// </summary>
        /// <param name="a">The assembly where to search subscribers</param>
        public void LoadSubscribers(Assembly a)
        {
            if (a == (Assembly)null)
                throw new ArgumentNullException("a");
            Type[] exportedTypes = a.GetExportedTypes();
            foreach (Type t in exportedTypes)
            {
                if (IsSubscriber(t))
                    Subscribers.Add(t);
            }
        }


        /// <summary>
        /// Searches a given file for types that are publishers or subscribers
        /// </summary>
        /// <param name="fi">Fileinfo pointing to the file to inspect, must be a .dll or .exe file</param>
        public void DiscoverModules(System.IO.FileInfo fi)
        {
            if (!fi.Exists)
                throw new FileNotFoundException("Assembly file not found");
            if (fi.Extension != ".exe" && fi.Extension != ".dll")
                throw new ArgumentException("Assembly must be a .exe or .dll");

            Assembly a = Assembly.LoadFrom(fi.FullName);
            LoadPublishers(a);
            LoadSubscribers(a);
        }

        /// <summary>
        /// Creates an instance of a module type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Class does not define static factory method defining the [Factory] Attribute</exception>
        public object CreateModuleInstance(Type type)
        {
            if (IsPublisher(type) || IsSubscriber(type))
            {
                var methodInfo = GetAttributedMethod(type, typeof(FactoryAttribute), true);
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
        /// Starts the publisher.
        /// </summary>
        /// <param name="instance">The instance of a publisher to be started.</param>
        /// <exception cref="System.ArgumentException">If No valid starter was method found</exception>
        /// <remarks>
        /// A publisher must define a parameterless method attributed with <see cref="Ella.Attributes.StartAttribute" />
        /// </remarks>
        public void StartPublisher(object instance)
        {
            var type = instance.GetType();
            if (IsPublisher(type))
            {
                var method = GetAttributedMethod(type, typeof(StartAttribute));
                if (method == null)
                    throw new ArgumentException("No valid starter method found");
                if (!method.GetParameters().Any())
                {
                    //TODO perform this in a separate thread
                    method.Invoke(instance, null);
                }
            }
        }

        #endregion
        #region private helpers

        /// <summary>
        /// Determines whether the specified t is publisher.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified t is publisher; otherwise, <c>false</c>.
        /// </returns>
        private bool IsPublisher(Type t)
        {
            return DefinesAttribute(t, typeof(PublishesAttribute));
        }

        private bool ArePublisherEventsValid(Type t)
        {
            var eventIds = (from a in
                                (IEnumerable<PublishesAttribute>)
                                t.GetCustomAttributes(typeof(PublishesAttribute), true)
                            select a.ID).GroupBy(i => i);
            foreach (var eventId in eventIds)
            {
                if (eventId.Count() > 1)
                    //throw new ArgumentException(string.Format("Publisher {0} defines ID {1} multiple times", t, eventId.Key));
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the specified t is subscriber.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified t is subscriber; otherwise, <c>false</c>.
        /// </returns>
        private bool IsSubscriber(Type t)
        {
            return DefinesAttribute(t, typeof(SubscriberAttribute));
        }

        /// <summary>
        /// Checks if <paramref name="t"/> defines the attribute <paramref name="attribute"/>.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        private bool DefinesAttribute(Type t, Type attribute)
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
        private MethodBase GetAttributedMethod(Type type, Type attribute, bool includeConstructors = false)
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
        #endregion


    }
}
