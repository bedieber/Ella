using System.Configuration;

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
    }
}
