using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Ella.Internal;
using Ella.Model;
using Ella.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ella
{
    [TestClass]
    public class NetworkTest
    {

        [TestInitialize]
        public void InitElla()
        {
            EllaModel.Instance.Reset();
        }

        [TestMethod]
        public void CreateGenericSubsription()
        {
            FakeProxy proxy = new FakeProxy();

            SubscriptionBase subscription = ReflectionUtils.CreateGenericSubscription(typeof(String), new Event(), proxy);
            Assert.IsInstanceOfType(subscription, typeof(Subscription<String>));
            Subscription<String> sub = subscription as Subscription<String>;
            sub.Callback("Hello", null);
            Assert.IsTrue(proxy.eventReceived);
        }

        [TestMethod]
        public void CreateGenericSubsriptionForValueType()
        {
            FakeProxy proxy = new FakeProxy();

            SubscriptionBase subscription = ReflectionUtils.CreateGenericSubscription(typeof(DateTime), new Event(), proxy);
            Assert.IsInstanceOfType(subscription, typeof(Subscription<DateTime>));
            Subscription<DateTime> sub = subscription as Subscription<DateTime>;
            sub.Callback(DateTime.Now, null);
            Assert.IsTrue(proxy.eventReceived);
        }

    }
}
