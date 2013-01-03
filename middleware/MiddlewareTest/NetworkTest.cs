using System;
using System.Collections.Generic;
using System.Linq;
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
        [TestMethod]
        public void CreateGenericSubsription()
        {
            FakeProxy p = new FakeProxy();
            SubscriptionBase subscription= ReflectionUtils.CreateGenericSubscription(typeof (String), new Model.Event(), p);
            Assert.IsInstanceOfType(subscription, typeof (Subscription<String>));
            Subscription<String> sub = subscription as Subscription<String>;
            sub.Callback("Hello");
            Assert.IsTrue(p.eventReceived);
        }

       
    }
}
