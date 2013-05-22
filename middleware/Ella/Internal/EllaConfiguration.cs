//=============================================================================
// Project  : Ella Middleware
// File    : EllaConfiguration.cs
// Authors contact  : Bernhard Dieber (Bernhard.Dieber@uni-klu.ac.at)
// Copyright 2012 by Bernhard Dieber, Jennifer Simonjan
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://ella.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//=============================================================================

using System;
using System.Configuration;
using System.Net;

namespace Ella.Internal
{
    /// <summary>
    /// Holds the configuration for ella
    /// </summary>
    public sealed class EllaConfiguration : ConfigurationSection
    {
        private static EllaConfiguration _instance;
        private static object _lock = new object();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        internal static EllaConfiguration Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = ConfigurationManager.GetSection("EllaConfiguration") as EllaConfiguration;
                    }
                }
                return _instance;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="EllaConfiguration" /> class.
        /// </summary>
        public EllaConfiguration()
        {

        }

        /// <summary>
        /// Gets or sets the node id.
        /// </summary>
        /// <value>
        /// The node id.
        /// </value>
        [ConfigurationProperty("NodeId", IsRequired = true, IsKey = true, DefaultValue = (int)1)]
        [IntegerValidator(MinValue = 0, MaxValue = 0, ExcludeRange = true)]
        public int NodeId
        {
            get { return (int)this["NodeId"]; }
            set { this["NodeID"] = value; }
        }

        /// <summary>
        /// Gets or sets the network port.
        /// </summary>
        /// <value>
        /// The network port.
        /// </value>
        [ConfigurationProperty("NetworkPort", DefaultValue = (int)33333, IsRequired = false)]
        [IntegerValidator(MinValue = 1, MaxValue = 65535, ExcludeRange = false)]
        public int NetworkPort
        {
            get { return (int)this["NetworkPort"]; }
            set { this["NetworkPort"] = value; }
        }

        /// <summary>
        /// Gets or sets the network port range start.
        /// </summary>
        /// <value>
        /// The network port range start.
        /// </value>
        [ConfigurationProperty("DiscoveryPortRangeStart", DefaultValue = (int)33333, IsRequired = false)]
        [IntegerValidator(MinValue = 1, MaxValue = 65535, ExcludeRange = false)]
        public int DiscoveryPortRangeStart
        {
            get { return (int)this["DiscoveryPortRangeStart"]; }
            set { this["DiscoveryPortRangeStart"] = value; }
        }

        /// <summary>
        /// Gets or sets the network port range stop.
        /// </summary>
        /// <value>
        /// The network port range stop.
        /// </value>
        [ConfigurationProperty("DiscoveryPortRangeEnd", DefaultValue = (int)33333, IsRequired = false)]
        [IntegerValidator(MinValue = 0, MaxValue = 65535, ExcludeRange = false)]
        public int DiscoveryPortRangeEnd
        {
            get { return (int)this["DiscoveryPortRangeEnd"]; }
            set { this["DiscoveryPortRangeEnd"] = value; }
        }

        /// <summary>
        /// Gets or sets the size of the port range.
        /// </summary>
        /// <value>
        /// The port range size.
        /// </value>
        [ConfigurationProperty("MulticastPortRangeSize", DefaultValue = (int)100, IsRequired = false)]
        [IntegerValidator(MinValue = 1, MaxValue = 65535, ExcludeRange = false)]
        public int MulticastPortRangeSize
        {
            get { return (int)this["MulticastPortRangeSize"]; }
            set { this["MulticastPortRangeSize"] = value; }
        }


        /// <summary>
        /// Gets or sets the Multicast address.
        /// </summary>
        /// <value>The multicast address.
        /// </value>
        [ConfigurationProperty("MulticastAddress", DefaultValue = "228.4.0.1")]
        [CallbackValidator(CallbackMethodName = "ValidateMulticastAddress", Type = typeof(EllaConfiguration))]
        public string MulticastAddress
        {
            get { return (string)this["MulticastAddress"]; }
            set { this["MulticastAddress"] = value; }
        }

        /// <summary>
        /// Validator method to check whether the string, holding the ip address, is a multicast address or not.
        /// </summary>
        /// <param name="o"></param>
        public static void ValidateMulticastAddress(object o)
        {
            string ipString = o as string;

            if (ipString == null)
                throw new ConfigurationErrorsException("MulticastAddress is not an string.");

            IPAddress ip = IPAddress.Parse(ipString);
            byte[] addressBytes = ip.GetAddressBytes();

            if (224 <= addressBytes[0] && addressBytes[0] <= 239)
            {
                return;
            }

            throw new ConfigurationErrorsException("MulticastAddress is not in a valid range.");
        }
    }
}
