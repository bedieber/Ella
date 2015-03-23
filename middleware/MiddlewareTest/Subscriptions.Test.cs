using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ella;
using Ella.Controller;
using Ella.Internal;
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
            Assert.AreEqual(2, EllaModel.Instance.TotalNumberOfSubscriptions);
            subscriber.Subscribe(); Assert.AreEqual(2, EllaModel.Instance.TotalNumberOfSubscriptions);
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
            Assert.AreEqual(1, EllaModel.Instance.TotalNumberOfSubscriptions);
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
            Assert.IsTrue(s.NewDataHandle.TrueForAll(h => s.SubscriptionCallBackHandle.Contains(h)));
        }

        [TestMethod]
        public void UnsubscribeFromType()
        {
            TestPublisher p = new TestPublisher();
            Start.Publisher(p);

            TestSubscriber s = new TestSubscriber();
            s.Subscribe();
            p.PublishEvent();
            Thread.Sleep(100);
            Assert.IsTrue(EllaModel.Instance.TotalNumberOfSubscriptions > 0);
            Thread.Sleep(100);

            s.UnsubscribeFromString();
            Assert.IsTrue(EllaModel.Instance.TotalNumberOfSubscriptions == 0);
        }

        [TestMethod]
        public void UnsubscribeByHandle()
        {
            TestPublisher p = new TestPublisher();
            Start.Publisher(p);

            TestSubscriber s = new TestSubscriber();

            s.Subscribe();
            p.PublishEvent();
            Thread.Sleep(100);
            int count = EllaModel.Instance.TotalNumberOfSubscriptions;
            Assert.IsTrue(count > 0);
            Thread.Sleep(100);

            s.UnsubscribeByHandle();
            Assert.IsTrue(EllaModel.Instance.TotalNumberOfSubscriptions == count - 1);
        }

        [TestMethod]
        public void UnsubscribeFromObject()
        {
            CopyPolicyFalse p = new CopyPolicyFalse();
            Start.Publisher(p);

            TestSubscriber s = new TestSubscriber();
            s.Subscribe();
            s.SubscribeToBool();

            Thread.Sleep(1000);

            s.UnsubscribeFromObject();

            Assert.AreEqual(0, EllaModel.Instance.TotalNumberOfSubscriptions);
        }

        [TestMethod]
        public void EventAssociationsAreDeliverdOnce()
        {
            TestPublisher p = new TestPublisher();
            Start.Publisher(p);
            TestSubscriber s = new TestSubscriber();
            s.Subscribe();
            Assert.AreEqual(2, s.NumAssociationsReceived);
        }

        [TestMethod]
        public void EventAssociationsAreDeliveredIndependentOfOrder()
        {
            //TODO this test will fail until local subscription caching has been implemented
            TestPublisher p = new TestPublisher();
            TestSubscriber s = new TestSubscriber();
            s.Subscribe();

            Start.Publisher(p);
            Thread.Sleep(1000);
            Assert.AreEqual(2, s.NumAssociationsReceived);
        }

        [TestMethod]
        public void AssociateEvents()
        {
            EventAssociationPublisher p = new EventAssociationPublisher();
            Start.Publisher(p);

            EventHandle handle = new EventHandle()
            {
                EventId = 1,
                PublisherId = EllaModel.Instance.GetPublisherId(p),
                PublisherNodeId = EllaConfiguration.Instance.NodeId,
            };

            Thread.Sleep(1000);

            IEnumerable<EventHandle> ev = EllaModel.Instance.GetEventCorrelations(handle);

            Assert.AreEqual(1, ev.Count());
            Assert.AreEqual(2, ev.ElementAt(0).EventId);
        }
        [TestMethod]
        public void PublisherWithCallbackMethod()
        {
            PublisherWithCallbackMethod p = new PublisherWithCallbackMethod();
            Start.Publisher(p);

            TestSubscriber s = new TestSubscriber();
            s.SubscribeForPublisherWithCallbackMethod();

            TestSubscriber x = new TestSubscriber();
            x.SubscribeForPublisherWithCallbackMethod();

            Thread.Sleep(1000);

            Assert.IsTrue(p.callback.Equals(2));
        }

        [TestMethod]
        public void PublishesEventsToSpecificSubscribers()
        {
            PublishesEventsToSpecificSubscribers p = new PublishesEventsToSpecificSubscribers();
            Start.Publisher(p);

            TestSubscriber s = new TestSubscriber();
            s.SubscribeForPublisherWithSpecificSubscribers();

            TestSubscriber x = new TestSubscriber();
            x.SubscribeForPublisherWithSpecificSubscribers();

            TestSubscriber y = new TestSubscriber();
            y.SubscribeForPublisherWithSpecificSubscribers();

            Thread.Sleep(1000);

            p.PublishEvent();
            Thread.Sleep(1000);
            Assert.AreEqual(1, s.numEventsReceived);
            Assert.AreEqual(0, x.numEventsReceived);
            Assert.AreEqual(1, y.numEventsReceived);
        }

        [TestMethod]
        public void LateStartOfPublisherResultsInValidSubscription()
        {
            TestSubscriber s = new TestSubscriber();
            s.Subscribe();
            Thread.Sleep(100);
            TestPublisher p = new TestPublisher();
            Start.Publisher(p);
            p.PublishEvent();
            Thread.Sleep(500);
            Assert.AreEqual(1, s.numEventsReceived);
        }

        [TestMethod]
        public void RemoteSubscriptionsWithSameReferenceAreRejected()
        {
            var tp = new PublisherWithCallbackMethod();
            Start.Publisher(tp);

            SubscriptionController.SubscribeRemoteSubscriber(typeof(bool), 2,
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 45000), 33);

            Assert.AreEqual(1, tp.callback);

            SubscriptionController.SubscribeRemoteSubscriber(typeof(bool), 2,
               new IPEndPoint(IPAddress.Parse("127.0.0.1"), 45000), 33);
            Assert.AreEqual(1, tp.callback);
        }

        [TestMethod]
        public void RemoteSubscriptionsWithSameReferenceFromDifferentNodesAreAccepted()
        {
            var tp = new PublisherWithCallbackMethod();
            Start.Publisher(tp);

            SubscriptionController.SubscribeRemoteSubscriber(typeof(bool), 2,
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 45000), 33);

            Assert.AreEqual(1, tp.callback);

            SubscriptionController.SubscribeRemoteSubscriber(typeof(bool), 3,
               new IPEndPoint(IPAddress.Parse("127.0.0.1"), 45000), 33);
            Assert.AreEqual(2, tp.callback);
        }

    }
}
