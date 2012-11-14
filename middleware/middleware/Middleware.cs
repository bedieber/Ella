using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Middleware.Attributes;

namespace Middleware
{
    /// <summary>
    /// Name to be refactored
    /// </summary>
    public class Middleware
    {
        public Middleware()
        {
            Publishers = new List<Type>();
        }

        public ICollection<Type> Publishers
        {
            get;
            set;
        }

        public void LoadPublishers(System.IO.FileInfo fi)
        {
                Assembly a = Assembly.LoadFile(fi.FullName);
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
                        if (attribute is PublisherAttribute)
                            Publishers.Add(t);
                    }
                }
        }
    }
}
