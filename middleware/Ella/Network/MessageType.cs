//=============================================================================
// Project  : Ella Middleware
// File    : Message.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@aau.at)
// Copyright 2013 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

namespace Ella.Network
{
    /// <summary>
    /// Defines the type of message contained
    /// </summary>
    internal enum MessageType : byte
    {
        Discover = 0,
        Subscribe = 1,
        Unsubscribe,
        SubscribeResponse,
        RequestTemplate,
        Publish,
        DiscoverResponse,
        ApplicationMessage,
        ApplicationMessageResponse,
        EventCorrelation,
        NodeShutdown
    }
}