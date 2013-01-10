using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            Assert.IsTrue(h == h);
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
            RemoteSubscriptionHandle h2 = new RemoteSubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3, RemoteNodeID = 15, SubscriptionReference=1 };
            Assert.AreNotEqual(h, h2);
            Assert.IsFalse(h == h2);
        }

        [TestMethod]
        public void SameRemoteAreEqual()
        {
            RemoteSubscriptionHandle h = new RemoteSubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3, RemoteNodeID = 15 };
            RemoteSubscriptionHandle h2 = new RemoteSubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3, RemoteNodeID = 15 };
            Assert.AreEqual(h, h2);
            Assert.IsTrue(h == h2);
        }

        [TestMethod]
        public void DifferentRemoteIdsAreNotEqual()
        {
            RemoteSubscriptionHandle h = new RemoteSubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3, RemoteNodeID = 15 };
            RemoteSubscriptionHandle h2 = new RemoteSubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3, RemoteNodeID = 14 };
            Assert.AreNotEqual(h, h2);
            Assert.IsTrue(h != h2);
        }

        [TestMethod]
        public void CastedRemoteAreNotEqual()
        {
            SubscriptionHandle h = new SubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3 };
            SubscriptionHandle h2 = new RemoteSubscriptionHandle { EventID = 1, SubscriberId = 2, PublisherId = 3, RemoteNodeID = 15 };
            Assert.AreNotEqual(h, h2);
            Assert.IsFalse(h == h2);
        }
    }
}
