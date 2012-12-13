using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Ella.Attributes;
using Ella.Exceptions;
using Ella.Internal;
using Ella.Model;
using Ella.Network;

namespace Ella
{
    /// <summary>
    /// This class is used to start publishers
    /// </summary>
    public static class Start
    {
        /// <summary>
        /// Starts the publisher.
        /// </summary>
        /// <param name="instance">The instance of a publisher to be started.</param>
        /// <exception cref="System.ArgumentException">If No valid starter was method found</exception>
        /// <remarks>
        /// A publisher must define a parameterless method attributed with <see cref="Ella.Attributes.StartAttribute" />
        /// </remarks>
        public static void Publisher(object instance)
        {
            var type = instance.GetType();
            if (Is.ValidPublisher(type))
            {
                var method = ReflectionUtils.GetAttributedMethod(type, typeof(StartAttribute));
                if (method == null)
                    throw new InvalidPublisherException("Publisher does not define a start method");
                if (!method.GetParameters().Any())
                {
                    EllaModel.Instance.ActivePublishers.Add(instance);
                    Thread t = new Thread(() => method.Invoke(instance, null));
                }
            }
            else
            {
                throw new InvalidPublisherException(instance.GetType().ToString());
            }
        }

        /// <summary>
        /// Starts the Ella network functionality<br />
        /// Unless this method is called, your Ella application will be local-only
        /// </summary>
        public static void Network()
        {
            NetworkController.Start();

        }


    }

    /// <summary>
    /// This class is used to stop running publishers
    /// </summary>
    public static class Stop
    {
        /// <summary>
        /// Stops a publisher.
        /// </summary>
        /// <param name="instance">The instance of a publisher to be stopped.</param>
        /// <exception cref="System.ArgumentException">If no valid stop method was found.</exception>
        /// <remarks>
        /// A publisher has to define a parameterless method attributed with <see cref="Ella.Attributes.StopAttribute" />
        /// </remarks>
        public static void Publisher(object instance)
        {
            var type = instance.GetType();
            if (Is.Publisher(type))
            {
                var method = ReflectionUtils.GetAttributedMethod(type, typeof(StopAttribute));

                if (method == null)
                    throw new InvalidPublisherException("No valid stop method found");

                if (!method.GetParameters().Any())
                {
                    method.Invoke(instance, null);
                    if (EllaModel.Instance.ActivePublishers.Contains(instance))
                        EllaModel.Instance.ActivePublishers.Remove(instance);
                }
                else
                {
                    throw new InvalidPublisherException("Stop method requires parameters, this is not supported");
                }
            }
        }
    }
}
