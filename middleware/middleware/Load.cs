using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ella.Model;

namespace Ella
{
    /// <summary>
    /// This class is used to load publishers and subscribers into the Ella system
    /// </summary>
    public static class Load
    {
        /// <summary>
        /// Loads all publishers from a given assembly and adds them to the Ella-internal management
        /// </summary>
        /// <param name="a">The assembly where to search publishers in</param>
        public static void Publishers(Assembly a)
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
                if (Is.ValidPublisher(t))
                {
                    if (!EllaModel.Instance.Publishers.Contains(t))
                        EllaModel.Instance.Publishers.Add(t);
                }
            }
        }
        /// <summary>
        /// Load all Subscribers from a given assembly and adds them to the Ella-internal management<br />
        /// <remarks>Any type must define the <see cref="Ella.Attributes.SubscriberAttribute"/> attribute in order to be detected as subcriber</remarks>
        /// </summary>
        /// <param name="a">The assembly where to search subscribers</param>
        public static void Subscribers(Assembly a)
        {
            if (a == (Assembly)null)
                throw new ArgumentNullException("a");

            Type[] exportedTypes = a.GetExportedTypes();
            foreach (Type t in exportedTypes)
            {
                if (Is.Subscriber(t))
                    EllaModel.Instance.Subscribers.Add(t);
            }
        }
    }
}
