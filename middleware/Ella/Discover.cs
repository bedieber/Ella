//=============================================================================
// Project  : Ella Middleware
// File    : Discover.cs
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;

namespace Ella
{
    /// <summary>
    /// This class aids in discovering modules (publishers or subscribers) in an assembly
    /// </summary>
    public static class Discover
    {
        /// <summary>
        /// Searches a given file for types that are publishers or subscribers
        /// </summary>
        /// <param name="fi">Fileinfo pointing to the file to inspect, must be a .dll or .exe file</param>
        public static void Modules(System.IO.FileInfo fi)
        {
            ILog log = LogManager.GetLogger(typeof (Discover));
            if (!fi.Exists)
            {
                log.ErrorFormat("Assembly file {0} not found", fi);
                throw new FileNotFoundException("Assembly file not found");
            }
            if (fi.Extension != ".exe" && fi.Extension != ".dll")
            {
                log.ErrorFormat("Assembly file {0} is neither .exe nor .dll", fi);
                throw new ArgumentException("Assembly must be a .exe or .dll");
            }

            Assembly a = Assembly.LoadFrom(fi.FullName);
            Load.Publishers(a);
            Load.Subscribers(a);
        }
    }
}
