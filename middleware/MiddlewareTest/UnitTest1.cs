using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MiddlewareTest
{
    [TestClass]
    public class TestMiddleware
    {
        [TestMethod]
        public void DiscoverPublishers()
        {
            Middleware.Middleware mw = new Middleware.Middleware();
            Assert.IsNotNull(mw.Publishers);
            FileInfo fi = new FileInfo(GetType().Assembly.Location);
            Assembly a = Assembly.LoadFile(fi.FullName);
            mw.LoadPublishers(a);
            Assert.IsTrue(mw.Publishers.Count() == 1);
        }

        [TestMethod]
        public void DiscoverSubscribers()
        {
            //TODO Jenny's first feature
        }

        [TestMethod]
        public void BuildSubscriberList()
        {
            Middleware.Middleware mw = new Middleware.Middleware();
            Assert.IsNotNull(mw.Subscribers);
            Assert.IsTrue(mw.Subscribers.Count == 0);
        }

        [TestMethod]
        public void DiscoverModulesInAssemblies()
        {
            FileInfo fi = new FileInfo(GetType().Assembly.Location);
            Middleware.Middleware mw = new Middleware.Middleware();
            mw.DiscoverModules(fi);
            Assert.IsTrue(mw.Subscribers.Count == 1);
            Assert.IsTrue(mw.Publishers.Count == 1);
        }

        [TestMethod]
        public void CreateInstanceOfAModule()
        {
            Middleware.Middleware mw=new Middleware.Middleware();
            object publisher=mw.CreateModuleInstance(typeof (TestPublisher));
            Assert.IsNotNull(publisher);
            Assert.IsInstanceOfType(publisher,typeof(TestPublisher));
            
            object subscriber = mw.CreateModuleInstance(typeof (TestSubscriber));
            Assert.IsNotNull(subscriber);
            Assert.IsInstanceOfType(subscriber, typeof(TestSubscriber));

        }
    }
}
