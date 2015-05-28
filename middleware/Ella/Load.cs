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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Ella.Attributes;
using Ella.Internal;
using Ella.Model;
using log4net;

namespace Ella
{
    /// <summary>
    /// This class is used to load publishers and subscribers into the Ella system
    /// </summary>
    public static class Load
    {
        private static ILog _log = LogManager.GetLogger(typeof(Load));

        /// <summary>
        /// Loads all publishers from a given assembly and adds them to the Ella-internal management
        /// </summary>
        /// <param name="a">The assembly where to search publishers in</param>
        /// <param name="createInstances">if <c>true</c>, instances of the found publisher types are created and started during the discovery process</param>
        /// <param name="activation"></param>
        public static void Publishers(Assembly a, bool createInstances = false, Func<Type, object> activation = null)
        {
            if (a == (Assembly)null)
                throw new ArgumentNullException("a");
            _log.DebugFormat("Loading publishers from {0}", a.FullName);
            AssemblyName[] referencedAssemblies = a.GetReferencedAssemblies();
            foreach (AssemblyName name in referencedAssemblies)
            {
                ResolveAndLoadAssembly(name);
            }

            Type[] exportedTypes = a.GetExportedTypes();
            foreach (Type t in exportedTypes)
            {
                if (Is.ValidPublisher(t))
                {
                    _log.DebugFormat("Found publisher {0} in assembly {1}", t, a.FullName);
                    if (!EllaModel.Instance.Publishers.Contains(t))
                    {
                        EllaModel.Instance.Publishers.Add(t);
                        if (createInstances)
                        {
                            var instance = Create.ModuleInstance(t, activation);
                            if (instance != null)
                                Start.Publisher(instance);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load all Subscribers from a given assembly and adds them to the Ella-internal management<br />
        /// <remarks>Any type must define the <see cref="Ella.Attributes.SubscriberAttribute"/> attribute in order to be detected as subcriber</remarks>
        /// </summary>
        /// <param name="a">The assembly where to search subscribers</param>
        /// <param name="createInstances">if <c>true</c>, instances of discovered subscribers are created</param>
        /// <param name="activation"></param>
        public static void Subscribers(Assembly a, bool createInstances = false, Func<Type, object> activation = null)
        {
            if (a == (Assembly)null)
                throw new ArgumentNullException("a");

            _log.DebugFormat("Loading subscribers from {0}", a.FullName);
            AssemblyName[] referencedAssemblies = a.GetReferencedAssemblies();

            foreach (AssemblyName name in referencedAssemblies)
            {
                   ResolveAndLoadAssembly(name);
            }
            Type[] exportedTypes = a.GetExportedTypes();
            foreach (Type t in exportedTypes)
            {
                if (Is.Subscriber(t))
                {
                    _log.DebugFormat("Found subscriber {0} in assembly {1}", t, a.FullName);
                    if (!EllaModel.Instance.Subscribers.Contains(t))
                        EllaModel.Instance.Subscribers.Add(t);
                    if (createInstances)
                        Create.ModuleInstance(t, activation);
                }
            }
        }

        /// <summary>
        /// Resolves the assembly.
        /// </summary>
        /// <param name="name">The name.</param>
        private static void ResolveAndLoadAssembly(AssemblyName name)
        {
            if (AppDomain.CurrentDomain.GetAssemblies().All(r => r.FullName != name.FullName))
            {
                DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());

                if (di.Exists)
                {
                    try
                    {
                        var assemblyName = new AssemblyName(name.FullName);
                        var files = di.GetFiles(string.Format("{0}.*", assemblyName.Name),
                            SearchOption.AllDirectories);
                        var fileInfos =
                            files
                                .Where(f => f.Extension == ".dll" || f.Extension == ".exe");
                        if (fileInfos.Any())
                        {
                            var assemblyFile = fileInfos.First();
                            var assembly = Load.Assembly(assemblyFile);

                            _log.DebugFormat("Assembly {0} loaded", assemblyFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.DebugFormat("Could not load referenced assembly {0}: {1}", name.FullName, ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Loads the specified assembly file into the runtime
        /// </summary>
        /// <param name="assemblyFile">The assembly file.</param>
        /// <returns></returns>
        public static Assembly Assembly(FileInfo assemblyFile)
        {
            var assemblyName = AssemblyName.GetAssemblyName(assemblyFile.FullName);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName == assemblyName.FullName);
            if (assemblies.Any())
            {
                _log.Debug("Assembly resolved from appdomain");
                return assemblies.First();
            }
            byte[] assemblyBytes = new byte[assemblyFile.Length];
            File.OpenRead(assemblyFile.FullName).Read(assemblyBytes, 0, assemblyBytes.Length);
            var assembly = System.Reflection.Assembly.Load(assemblyBytes);
            
            return assembly;
        }
    }
}
