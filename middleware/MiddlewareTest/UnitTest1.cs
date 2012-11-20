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
        private const int NumPublishers=1;
        private const int NumSubscribers = 5;
        [TestMethod]
        public void DiscoverPublishers()
        {
            Middleware.Middleware mw = new Middleware.Middleware();
            Assert.IsNotNull(mw.Publishers);
            FileInfo fi = new FileInfo(GetType().Assembly.Location);
            Assembly a = Assembly.LoadFile(fi.FullName);
            mw.LoadPublishers(a);
            Assert.IsTrue(mw.Publishers.Count() == NumPublishers);
        }

        [TestMethod]
        public void DiscoverSubscribers()
        {
            Middleware.Middleware m = new Middleware.Middleware();
            Assert.IsNotNull(m.Subscribers);
            FileInfo fi = new FileInfo(GetType().Assembly.Location);
            Assembly a = Assembly.LoadFile(fi.FullName);
            m.LoadSubscribers(a);
            Assert.IsTrue(m.Subscribers.Count() == NumSubscribers);
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
            Assert.IsTrue(mw.Subscribers.Count == NumSubscribers);
            Assert.IsTrue(mw.Publishers.Count == NumPublishers);
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
            try
            {
                subscriber = mw.CreateModuleInstance(typeof(TestSubscriberMethodFactory));
                Assert.Fail();
            }
            catch (ArgumentException)
            {

            }
            catch { Assert.Fail(); }
            
            try
            {
                subscriber = mw.CreateModuleInstance(typeof(TestSubscriberNoFactory));
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                
            }
            catch{Assert.Fail();}

            subscriber = mw.CreateModuleInstance(typeof(TestSubscriberStaticMethodFactory));
            Assert.IsNotNull(subscriber);
            Assert.IsInstanceOfType(subscriber, typeof(TestSubscriberStaticMethodFactory));

            try
            {
                subscriber = mw.CreateModuleInstance(typeof(TestSubscriberStaticConstructerFactory));
                Assert.Fail();
            }
            catch (ArgumentException)
            {

            }
            catch { Assert.Fail(); }
        }
    }
}
