﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ella
{
    [TestClass]
    public class HandleTest
    {

        [TestMethod]
        public void SelfEquals()
        {
            SubscriptionHandle h = new SubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3 };
            Assert.AreEqual(h, h);
// ReSharper disable EqualExpressionComparison
            Assert.IsTrue(h == h);
// ReSharper restore EqualExpressionComparison
        }
        [TestMethod]
        public void HandleEqualsNull()
        {
            SubscriptionHandle handle = null;
            Assert.IsTrue(handle == null);
        }
        [TestMethod]
        public void EqualsSameValues()
        {
            SubscriptionHandle h = new SubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3 };
            SubscriptionHandle h2 = new SubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3 };
            Assert.AreEqual(h, h2);
            Assert.IsTrue(h == h2);
        }

        [TestMethod]
        public void DifferentValues()
        {
            SubscriptionHandle h = new SubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3 };
            SubscriptionHandle h2 = new SubscriptionHandle { EventID = 3, SubscriberId = 2, PublisherId = 3 };
            Assert.AreNotEqual(h, h2);
            Assert.IsFalse(h == h2);
        }

        [TestMethod]
        public void RemoteAndLocalAreNotEqual()
        {
            SubscriptionHandle h = new SubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3 };
            RemoteSubscriptionHandle h2 = new RemoteSubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3, PublisherNodeID = 15, SubscriptionReference = 1, SubscriberNodeID = 1 };
            Assert.AreNotEqual(h, h2);
            Assert.IsFalse(h.Equals(h2));
            Assert.IsFalse(h == h2);
        }

        [TestMethod]
        public void SameRemoteAreEqual()
        {
            RemoteSubscriptionHandle h = new RemoteSubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3, PublisherNodeID = 15 };
            RemoteSubscriptionHandle h2 = new RemoteSubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3, PublisherNodeID = 15 };
            Assert.AreEqual(h, h2);
            Assert.IsTrue(h == h2);
        }

        [TestMethod]
        public void DifferentRemoteIdsAreNotEqual()
        {
            RemoteSubscriptionHandle h = new RemoteSubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3, PublisherNodeID = 15 };
            RemoteSubscriptionHandle h2 = new RemoteSubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3, PublisherNodeID = 14 };
            Assert.AreNotEqual(h, h2);
            Assert.IsTrue(h != h2);
        }

        [TestMethod]
        public void CastedRemoteAreNotEqual()
        {
            SubscriptionHandle h = new SubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3 };
            SubscriptionHandle h2 = new RemoteSubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3, PublisherNodeID = 15 };
            Assert.AreNotEqual(h, h2);
            Assert.IsFalse(h == h2);
        }

        [TestMethod]
        public void SamePublisherDifferentSubscriberNodeAreNotEqual()
        {
            RemoteSubscriptionHandle h = new RemoteSubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3, PublisherNodeID = 15, SubscriberNodeID = 1 };
            RemoteSubscriptionHandle h2 = new RemoteSubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3, PublisherNodeID = 15, SubscriberNodeID = 2 };
            Assert.AreNotEqual(h, h2);
            Assert.IsFalse(h == h2);
        }

        [TestMethod]
        public void SamePublisherAndSubscriberNodeDifferentSubscriberAreNotEqual()
        {
            RemoteSubscriptionHandle h = new RemoteSubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3, PublisherNodeID = 15, SubscriberNodeID = 1, SubscriptionReference = 35 };
            RemoteSubscriptionHandle h2 = new RemoteSubscriptionHandle { EventID = 1, SubscriberId = 3, PublisherId = 3, PublisherNodeID = 15, SubscriberNodeID = 1, SubscriptionReference = 34 };
            Assert.AreNotEqual(h, h2);
            Assert.IsFalse(h == h2);
        }
        
        
        [TestMethod]
        public void MulticastRemoteSubscriptionHandleDeserializedAsRemoteSubscriptionHandle()
        {
            RemoteSubscriptionHandle h1 = new RemoteSubscriptionHandle
                {
                    EventID = 1,
                    SubscriberId = 2,
                    PublisherId = 3,
                    PublisherNodeID = 15,
                    SubscriberNodeID = 1,
                    SubscriptionReference = 35
                };

            MulticastRemoteSubscriptionhandle h2 = new MulticastRemoteSubscriptionhandle
                {
                    EventID = 2,
                    SubscriberId = 3,
                    PublisherId = 1,
                    PublisherNodeID = 13,
                    SubscriberNodeID = 4,
                    SubscriptionReference = 30,
                    IpAddress = "228.4.0.1",
                    Port = 44550
                };

            List<RemoteSubscriptionHandle> list = new List<RemoteSubscriptionHandle>();
            list.Add(h1);
            list.Add(h2);

            byte [] serialized = Ella.Internal.Serializer.Serialize(list);

            List<RemoteSubscriptionHandle> list2 = Ella.Internal.Serializer.Deserialize<List<RemoteSubscriptionHandle>>(serialized);

            Assert.IsInstanceOfType(list2[1],typeof(MulticastRemoteSubscriptionhandle));
        }
    }
}
