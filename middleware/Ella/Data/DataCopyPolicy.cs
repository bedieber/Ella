//=============================================================================
// Project  : Ella Middleware
// File    : DataCopyPolicy.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

namespace Ella.Data
{
    /// <summary>
    /// This attribute is used by publishers to indicate whether their published data shall be copied before it's modified
    /// </summary>
    public enum DataCopyPolicy
    {
        /// <summary>
        /// If no copying of data before further processing is necessary<br />
        /// I.e. data is not further modified by the publisher and can be modified in-place by the subscriber
        /// </summary>
        None,
        /// <summary>
        /// Data should be copied before processing <br />
        /// I.e. the publisher will reuse the data contained in further processing steps
        /// </summary>
        Copy
    }
}