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
                    Publishers.Add(t);
            }
        }
        /// <summary>
        /// Load all Subscribers from a given assembly
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
                IEnumerable<MethodBase> methodInfos = type.GetMethods();
                var constructorInfos = type.GetConstructors();
                methodInfos = methodInfos.Concat(constructorInfos);
                foreach (var methodInfo in methodInfos)
                {
                    var customAttributes = methodInfo.GetCustomAttributes(typeof(Attribute), true);
                    if (customAttributes.Any() && (methodInfo is ConstructorInfo || (methodInfo is MethodInfo && methodInfo.IsStatic)))
                    {
                        object instance = null;
                        if (methodInfo is ConstructorInfo)
                            instance = (methodInfo as ConstructorInfo).Invoke(null);
                        else
                            instance = methodInfo.Invoke(new object[] { }, null);
                        return instance;
                    }
                }
            }
            throw new ArgumentException("Class does not define static factory method defining the [Factory] Attribute");
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
            List<object> atr = new List<object>(t.GetCustomAttributes(true));

            foreach (var a in atr)
            {
                if (a.GetType() == attribute)
                    return true;
            }
            return false;
        }
        #endregion
    }
}
