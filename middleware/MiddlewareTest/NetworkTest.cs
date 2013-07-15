using System;
using System.Configuration;
using System.Net;
using System.Threading;
using Ella.Control;
using Ella.Controller;
using Ella.Fakes;
using Ella.Internal;
using Ella.Model;
using Ella.Network;
using Ella.Network.Communication;
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

        //[TestMethod]
        //public void CreateGenericSubsription()
        //{
        //    FakeProxy proxy = new FakeProxy();

        //    SubscriptionBase subscription = ReflectionUtils.CreateGenericSubscription(typeof(Image<Bgr,byte>), new Event(), proxy);
        //    Assert.IsInstanceOfType(subscription, typeof(Subscription<Image<Bgr, byte>>));
        //    Subscription<Image<Bgr, byte>> sub = subscription as Subscription<Image<Bgr, byte>>;
        //    sub.Callback(new Image<Bgr, byte>(320,240), null);
        //    Assert.IsTrue(proxy.eventReceived);
        //}

        //[TestMethod]
        //public void CreateGenericSubsriptionForValueType()
        //{
        //    FakeProxy proxy = new FakeProxy();

        //    SubscriptionBase subscription = ReflectionUtils.CreateGenericSubscription(typeof(DateTime), new Event(), proxy);
        //    Assert.IsInstanceOfType(subscription, typeof(Subscription<DateTime>));
        //    Subscription<DateTime> sub = subscription as Subscription<DateTime>;
        //    sub.Callback(DateTime.Now, null);
        //    Assert.IsTrue(proxy.eventReceived);
        //}

        [TestMethod]
        public void UnsubscribeFromNetwork()
        {
            throw new NotImplementedException();

            TestPublisher p = new TestPublisher();
            Start.Publisher(p);
            SubscriptionController.SubscribeRemoteSubscriber(typeof (string), 1, null, 3);
            Assert.IsTrue(EllaModel.Instance.Subscriptions.Count == 2);
            Message m = new Message(3);
            //ProcessUnsubscribe(new MessageEventArgs(m));
            Assert.IsTrue(EllaModel.Instance.Subscriptions.Count == 0);
        }

        [TestMethod]
        public void MulticastAddressIsInMulticastRange()
        {
           string ip = "230.255.255.4";
           EllaConfiguration.ValidateMulticastAddress(ip);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void MulticastAddressIsNotInMulticastRange()
        {
                string ip = "241.255.255.24";
                EllaConfiguration.ValidateMulticastAddress(ip);
        }

        [TestMethod]
        public void SubscriptionOverNetworkIsPerfomedOnClient()
        {
            FakeNetworkController nc=new FakeNetworkController();
            Networking.NetworkController = nc;
            Networking.Start();

            TestSubscriber ts=new TestSubscriber();
            ts.Subscribe();

            TestSubscriber s = new TestSubscriber();
            s.Subscribe();

            Assert.AreEqual(2,FakeNetworkController.Subscriptions[typeof(string)]);

        }

        [TestMethod]
        public void ApplicationMessageIsSentOverNetwork()
        {
            FakeNetworkController nc = new FakeNetworkController();
            Networking.NetworkController = nc;
            Networking.Start();

            TestPublisher p = new TestPublisher();
            Start.Publisher(p);

            TestSubscriber s = new TestSubscriber();
            s.Subscribe();

            Thread.Sleep(1000);

            p.PublishEvent();

            byte[] b={1,2};

            ApplicationMessage msg = new ApplicationMessage();
            msg.Data = b;

            Send.Message(msg, new RemoteSubscriptionHandle(), s);

            Assert.AreEqual(1,FakeNetworkController.countMsg);
        }

        [TestMethod]
        public void UnsubscribeOverNetwork()
        {
            FakeNetworkController nc = new FakeNetworkController();
            Networking.NetworkController = nc;
            Networking.Start();

            TestSubscriber s = new TestSubscriber();
            s.Subscribe();

            Thread.Sleep(1000);

            s.UnsubscribeFromRemote();

            Assert.IsTrue(FakeNetworkController.unsubscribed);
        }

        [TestMethod]
        public void ConnectToMulticastGroupOverNetwork()
        {
            FakeNetworkController nc = new FakeNetworkController();
            Networking.NetworkController = nc;
            Networking.Start();

            MulticastRemoteSubscriptionhandle h = new MulticastRemoteSubscriptionhandle();

            Networking.ConnectToMulticast(h.IpAddress,h.Port);

            TestSubscriber s = new TestSubscriber();
            s.Subscribe();

            Assert.IsTrue(FakeNetworkController.connectedToMulticastgroup);
        }

        [TestMethod]
        public void StartNetworking()
        {
            FakeNetworkController nc = new FakeNetworkController();
            nc.Start();

            Assert.IsTrue(FakeNetworkController.started);
        }

        [TestMethod]
        public void SendShutDownMessageOverNetwork()
        {
            FakeNetworkController nc = new FakeNetworkController();
            Networking.NetworkController = nc;
            Networking.Start();

            Stop.Ella();

            Assert.IsTrue(FakeNetworkController.shutDownMsgSent);
        }
    }
}
