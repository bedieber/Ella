# The Ella Configuration File

The Ella Config file enables adjusting some parameters you will or might need. The EllaConfiguration tag in the config file allows you to choose the following parameters:

* NodeId (a globally unique id of this application instance)
* NetworkPort (the network port to listen on for new traffic)
* DiscoveryPortRangeStart, DiscoveryPortRangeEnd (the range of the ports to be used in discovering new ella instances in the network)
* MulticastAddress (an optional multicast adress in case you want to use multicast traffic)
* MulticastPortRangeSize (the number of ports to use in multicasting)
* MTU (Maximum Transmission Unit)
* MaxQueueSize (maximum buffer size for the IpSender, if this size is reached the items currently in queue will be dropped)
Each parameter has also a default value, which means you don’t have to choose all of the variables. A sample config file can be found [here](ConfigFile_app.xml).



