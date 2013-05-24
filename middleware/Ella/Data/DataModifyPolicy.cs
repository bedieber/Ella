//=============================================================================
// Project  : Ella Middleware
// File    : DataModifyPolicy.cs
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
using System.Text;

namespace Ella.Data
{
    /// <summary>
    /// This attribute is used by subscribers to indicate whether they modify the published data or not
    /// </summary>
    public enum DataModifyPolicy
    {
        /// <summary>
        /// Data is not modified by the subscriber <br />
        /// </summary>
        NoModify,
        /// <summary>
        /// The data will be modified by the subscriber <br />
        /// </summary>
        Modify
    }
}
