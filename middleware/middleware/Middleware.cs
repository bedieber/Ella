using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Middleware.Attributes;

namespace Middleware
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

        /// <summary>
        /// Loads all publishers from a given assembly
        /// </summary>
        /// <param name="a">The assembly where to search publishers in</param>
        public void LoadPublishers(Assembly a)
        {
            //AssemblyName[] referencedAssemblies = a.GetReferencedAssemblies();
            //foreach (AssemblyName name in referencedAssemblies)
            //{
            //    Assembly.Load(name);
            //}
            Type[] exportedTypes = a.GetExportedTypes();
            foreach (Type t in exportedTypes)
            {
                List<object> atr = new List<object>(t.GetCustomAttributes(true));

                foreach (var attribute in atr)
                {
                    if (attribute is PublishesAttribute)
                        Publishers.Add(t);
                }
            }
        }
        /// <summary>
        /// Load all Subscribers from a given assembly
        /// </summary>
        /// <param name="a">The assembly where to search subscribers</param>
        public void LoadSubscribers(Assembly a)
        {
            Type[] exportedTypes = a.GetExportedTypes();
            foreach (Type t in exportedTypes)
            {
                List<object> atr = new List<object>(t.GetCustomAttributes(true));

                foreach (var attribute in atr)
                {
                    if (attribute is SubscriberAttribute)
                        Subscribers.Add(t);
                }
            }
        }


        public void DiscoverModules(System.IO.FileInfo fi)
        {
            if (!fi.Exists)
                throw new FileNotFoundException("Assembly file not found");
            if (fi.Extension != ".exe" && fi.Extension != ".dll")
                throw new ArgumentException("Assembly must be a .exe or .dll");

            Assembly a = Assembly.LoadFrom(fi.FullName);
            LoadPublishers(a);
            //TODO load subscribers

        }
    }
}
