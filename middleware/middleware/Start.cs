using System.Threading;
using Ella.Attributes;
using Ella.Exceptions;
using Ella.Internal;
using Ella.Model;
using Ella.Network;
using log4net;

namespace Ella
{
    /// <summary>
    /// This class is used to start publishers
    /// </summary>
    public static class Start
    {
        private static ILog _log = LogManager.GetLogger(typeof(Start));
        /// <summary>
        /// Starts the publisher.
        /// </summary>
        /// <param name="instance">The instance of a publisher to be started.</param>
        /// <exception cref="System.ArgumentException">If no valid starter was method found</exception>
        /// <remarks>
        /// A publisher must define a parameterless method attributed with <see cref="Attributes.StartAttribute" />
        /// </remarks>
        public static void Publisher(object instance)
        {
            Publisher publisher = ReflectionUtils.CreatePublisher(instance);
            if (publisher == null)
                throw new InvalidPublisherException(string.Format("{0} is not a valid publisher", instance));
            EllaModel.Instance.AddActivePublisher(publisher);
            _log.InfoFormat("Starting publisher {0}", EllaModel.Instance.GetPublisherId(instance));
            Thread t = new Thread(() => publisher.StartMethod.Invoke(instance, null));
            EllaModel.Instance.PublisherThreads.Add(t);
            t.Start();
        }

        /// <summary>
        /// Starts the Ella network functionality<br />
        /// Unless this method is called, your Ella application will be local-only
        /// </summary>
        public static void Network()
        {
            _log.Info("Starting network controller");
            NetworkController.Start();
        }

        /// <summary>
        /// Initializes the Ella Middleware system
        /// </summary>
        public static void Ella()
        {
            //  XmlConfigurator.ConfigureAndWatch(new FileInfo(
            //Path.GetDirectoryName(
            //      Assembly.GetAssembly(typeof(Start)).Location)
            //     + @"\" + "Ella.dll.config"));
            //  _log.Info("Ella started");
        }


    }
}
