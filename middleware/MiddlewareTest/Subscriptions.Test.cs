using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ella;
using Ella.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ella
{
    [TestClass]
    public class Subscriptions
    {
        
        [TestInitialize]
        public void InitElla()
        {
            EllaModel.Instance.Reset();
        }

        [TestMethod]
        public void ProvideTemplateObject()
        {
            TestPublisher t = new TestPublisher();
            Assert.IsInstanceOfType(Create.TemplateObject(t, 1), typeof(string));
            Assert.IsInstanceOfType(Create.TemplateObject(new TestPublisherPropertyTemplate(), 1), typeof(int));
            Assert.IsInstanceOfType(Create.TemplateObject(new TestPublisherMultipleEvents(), 2), typeof(string));
        }

        [TestMethod]
        public void DeliverEventToSubscribers()
        {
            TestPublisher publisher = new TestPublisher();
            Start.Publisher(publisher);
            TestSubscriber subscriber = new TestSubscriber();
            subscriber.Subscribe();
            publisher.PublishEvent();
            Thread.Sleep(100);
            Assert.AreEqual(subscriber.numEventsReceived, 1);
        }

        [TestMethod]
        public void SubscribeToEventByType()
        {
            TestPublisher pub = new TestPublisher();
            Start.Publisher(pub);
            TestSubscriber subscriber = new TestSubscriber();
            subscriber.Subscribe();
            Assert.AreEqual(2, EllaModel.Instance.Subscriptions.Count());
            subscriber.Subscribe();
            Assert.AreEqual(2, EllaModel.Instance.Subscriptions.Count());
        }

        [TestMethod]
        public void SubscribeToEventByTemplateObject()
        {
            ValueType vt = DateTime.Now;
            DateTime dt = (DateTime)vt;
            Console.WriteLine(dt);

            TestPublisher pub = new TestPublisher();
            Start.Publisher(pub);
            TestSubscriber subscriber = new TestSubscriber();
            subscriber.SubscribeWithObject();
            Assert.AreEqual(1, EllaModel.Instance.Subscriptions.Count());
        }

        [TestMethod]
        public void SubscriptionHandleIsEqualOnNotificationAndReceivedEvent()
        {
            TestPublisher tp = new TestPublisher();
            Start.Publisher(tp);
            TestSubscriber s = new TestSubscriber();
            s.Subscribe();
            tp.PublishEvent();
            Thread.Sleep(100);
            Assert.IsTrue(s.NewDataHandle.Count > 0);
            Assert.IsTrue(s.SubscriptionCallBackHandle.Count > 0);
            Assert.IsTrue(s.NewDataHandle.TrueForAll(h=>s.SubscriptionCallBackHandle.Contains(h)));
            
        }
    }
}
