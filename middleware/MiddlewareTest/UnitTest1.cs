using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Ella;
using Ella.Internal;
using Ella.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ella
{
    [TestClass]
    public class TestMiddleware
    {
        private const int NumPublishers = 6;
        private const int NumSubscribers = 5;
        
        [TestInitialize]
        public void InitElla()
        {
            EllaModel.Instance.Reset();
        }

        [TestMethod]
        public void DiscoverPublishers()
        {
            Assert.IsNotNull(EllaModel.Instance.Publishers);
            FileInfo fi = new FileInfo(GetType().Assembly.Location);
            Assembly a = Assembly.LoadFile(fi.FullName);
            Load.Publishers(a);
            Assert.AreEqual(NumPublishers - 1, EllaModel.Instance.Publishers.Count());
            Load.Publishers(a);
            Assert.AreEqual(NumPublishers - 1, EllaModel.Instance.Publishers.Count());
        }

        [TestMethod]
        public void DiscoverSubscribers()
        {
            Assert.IsNotNull(EllaModel.Instance.Subscribers);
            FileInfo fi = new FileInfo(GetType().Assembly.Location);
            Assembly a = Assembly.LoadFile(fi.FullName);
            Load.Subscribers(a);
            Assert.IsTrue(EllaModel.Instance.Subscribers.Count() == NumSubscribers);
        }

        [TestMethod]
        public void BuildSubscriberList()
        {
            Assert.IsNotNull(EllaModel.Instance.Subscribers);
            Assert.AreEqual(0, EllaModel.Instance.Subscribers.Count);
        }

        [TestMethod]
        public void DiscoverModulesInAssemblies()
        {
            FileInfo fi = new FileInfo(GetType().Assembly.Location);
            Discover.Modules(fi);
            Assert.AreEqual(NumSubscribers, EllaModel.Instance.Subscribers.Count);
            Assert.AreEqual(NumPublishers - 1, EllaModel.Instance.Publishers.Count);
        }

        [TestMethod]
        public void CreateInstanceOfAModule()
        {
            object publisher = Create.ModuleInstance(typeof(TestPublisher));
            Assert.IsNotNull(publisher);
            Assert.IsInstanceOfType(publisher, typeof(TestPublisher));

            object subscriber = Create.ModuleInstance(typeof(TestSubscriber));
            Assert.IsNotNull(subscriber);
            Assert.IsInstanceOfType(subscriber, typeof(TestSubscriber));
            try
            {
                subscriber = Create.ModuleInstance(typeof(TestSubscriberMethodFactory));
                Assert.Fail();
            }
            catch (ArgumentException)
            {

            }

            try
            {
                subscriber = Create.ModuleInstance(typeof(TestSubscriberNoFactory));
                Assert.Fail();
            }
            catch (ArgumentException)
            {

            }


            subscriber = Create.ModuleInstance(typeof(TestSubscriberStaticMethodFactory));
            Assert.IsNotNull(subscriber);
            Assert.IsInstanceOfType(subscriber, typeof(TestSubscriberStaticMethodFactory));

            try
            {
                subscriber = Create.ModuleInstance(typeof(TestSubscriberStaticConstructerFactory));
                Assert.Fail();
            }
            catch (ArgumentException)
            {

            }

        }


        [TestMethod]
        public void StartAPublisher()
        {
            object instance = Create.ModuleInstance(typeof(TestPublisher));
            Start.Publisher(instance);
        }

        [TestMethod]
        public void StopAPublisher()
        {
            object instance = Create.ModuleInstance(typeof(TestPublisher));
            Stop.Publisher(instance);
        }

        [TestMethod]
        public void ProvideTemplateObject()
        {
            // mw.GetTemplateObject(typeof(TestPublisher), 1);
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
            TestPublisher pub = new TestPublisher();
            Start.Publisher(pub);
            TestSubscriber subscriber = new TestSubscriber();
            subscriber.SubscribeWithObject();
            Assert.AreEqual(1, EllaModel.Instance.Subscriptions.Count());
        }

        [TestMethod]
        public void AdhereToCopyAndModificationPolicyTrueTrue()
        {
            //DataCopyPolicy.Copy & DataModifyPolicy.Modify
            CopyPolicyTrue t = new CopyPolicyTrue();
            Start.Publisher(t);
            TestSubscriber s = new TestSubscriber();
            s.SubscribeWithModifyTrue();
            t.PublishEvent();
            Assert.IsFalse(ReferenceEquals(t.data, s.rec));
        }

        [TestMethod]
        public void AdhereToCopyAndModificationPolicyFalseFalse()
        {
            //DataCopyPolicy.None & DataModifyPolicy.NoModify
            CopyPolicyFalse t = new CopyPolicyFalse();
            Start.Publisher(t);
            TestSubscriber s = new TestSubscriber();
            s.SubscribeWithModifyFalse();
            t.PublishEvent();
            Assert.IsTrue(ReferenceEquals(t.data,s.rec));
        }

        [TestMethod]
        public void AdhereToCopyAndModificationPolicyTrueFalse()
        {
            //DataCopyPolicy.Copy & DataModifyPolicy.NoModify
            CopyPolicyTrue t = new CopyPolicyTrue();
            Start.Publisher(t);
            TestSubscriber s = new TestSubscriber();
            s.SubscribeWithModifyFalse();
            t.PublishEvent();
            Assert.IsFalse(ReferenceEquals(t.data, s.rec));
        }

        [TestMethod]
        public void AdhereToCopyAndModificationPolicyFalseTrue()
        {
            //DataCopyPolicy.None & DataModifyPolicy.Modify
            CopyPolicyFalse t = new CopyPolicyFalse();
            Start.Publisher(t);
            TestSubscriber s = new TestSubscriber();
            s.SubscribeWithModifyTrue();
            t.PublishEvent();
            Assert.IsFalse(ReferenceEquals(t.data, s.rec));
        }

    }
}
