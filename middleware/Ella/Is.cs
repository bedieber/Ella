//=============================================================================
// Project  : Ella Middleware
// File    : Is.cs
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
using System.Linq;
using System.Text;
using Ella.Attributes;
using Ella.Internal;

namespace Ella
{
    /// <summary>
    /// This class provides helper functions for discovering publishers and subscribers
    /// </summary>
    public static class Is
    {
        #region Public helpers
        /// <summary>
        /// Determines whether the specified t is publisher.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified t is publisher; otherwise, <c>false</c>.
        /// </returns>
        public static bool Publisher(Type t)
        {
            return ReflectionUtils.DefinesAttribute(t, typeof(PublishesAttribute));
        }
        /// <summary>
        /// Checks whether the specified type is a valid publisher.
        /// </summary>
        /// <param name="t">The type</param>
        /// <returns>
        ///   <c>true</c> if the specified t is a valid publisher; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// <summary>
        /// A valid publisher must fulfill the following criteria
        /// <list type="bullet">
        /// <item><description>No multiply-defined event IDs are allowed.</description></item>
        /// <item><description>A start method has to be defined.</description></item>
        /// <item><description>A stop method has to be defined.</description></item>
        /// </list>
        /// </summary>
        /// </remarks>
        public static bool ValidPublisher(Type t)
        {
            //Check definition of publisher attribute
            if (!Is.Publisher(t))
                return false;

            //Check for multiply-defined event IDs
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

            //Check for start and stop Methods
            if (ReflectionUtils.GetAttributedMethod(t, typeof(StartAttribute)) == null)
                return false;
            if (ReflectionUtils.GetAttributedMethod(t, typeof(StopAttribute)) == null)
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether the specified t is subscriber.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified t is subscriber; otherwise, <c>false</c>.
        /// </returns>
        public static bool Subscriber(Type t)
        {
            return ReflectionUtils.DefinesAttribute(t, typeof(SubscriberAttribute));
        }
        #endregion
    }
}
