//=============================================================================
// Project  : Ella Middleware
// File    : Load.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
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
using Ella.Model;
using log4net;

namespace Ella
{
    /// <summary>
    /// This class is used to load publishers and subscribers into the Ella system
    /// </summary>
    public static class Load
    {
        private static ILog _log = LogManager.GetLogger(typeof (Load));
        /// <summary>
        /// Loads all publishers from a given assembly and adds them to the Ella-internal management
        /// </summary>
        /// <param name="a">The assembly where to search publishers in</param>
        public static void Publishers(Assembly a)
        {
            if (a == (Assembly)null)
                throw new ArgumentNullException("a");
            _log.DebugFormat("Loading publishers from {0}", a.FullName);
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
                    _log.DebugFormat("Found publisher {0} in assembly {1}", t, a.FullName);
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

            _log.DebugFormat("Loading subscribers from {0}", a.FullName);
            Type[] exportedTypes = a.GetExportedTypes();
            foreach (Type t in exportedTypes)
            {
                if (Is.Subscriber(t))
                {
                    _log.DebugFormat("Found subscriber {0} in assembly {1}", t, a.FullName);
                    EllaModel.Instance.Subscribers.Add(t);
                }
            }
        }
    }
}
