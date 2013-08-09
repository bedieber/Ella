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
        public void FakeNetworkControllerSubscription()
        {
            FakeNetworkController nc = new FakeNetworkController();
            Networking.NetworkController = nc;
            Networking.Start();

            TestSubscriber ts = new TestSubscriber();
            ts.Subscribe();

            TestSubscriber s = new TestSubscriber();
            s.Subscribe();

            Assert.AreEqual(2, FakeNetworkController.Subscriptions[typeof(string)]);
        }

        [TestMethod]
        public void FakeNetworkControllerApplicationMessageIsSent()
        {
            FakeNetworkController nc = new FakeNetworkController();
            Networking.NetworkController = nc;
            Networking.Start();

            //TestPublisher p = new TestPublisher();
            //Start.Publisher(p);

            TestSubscriber s = new TestSubscriber();
            s.Subscribe();

            //Thread.Sleep(1000);

            //p.PublishEvent();

            byte[] b = { 1, 2 };

            ApplicationMessage msg = new ApplicationMessage();
            msg.Data = b;

            Send.Message(msg, new RemoteSubscriptionHandle(), s);

            Assert.AreEqual(1, FakeNetworkController.countMsg);
        }

        [TestMethod]
        public void FakeNetworkControllerUnsubscribe()
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
        public void FakeNetworkControllerConnectToMulticastGroup()
        {
            FakeNetworkController nc = new FakeNetworkController();
            Networking.NetworkController = nc;
            Networking.Start();

            MulticastRemoteSubscriptionhandle h = new MulticastRemoteSubscriptionhandle();

            Networking.ConnectToMulticast(h.IpAddress, h.Port);

            TestSubscriber s = new TestSubscriber();
            s.Subscribe();

            Assert.IsTrue(FakeNetworkController.connectedToMulticastgroup);
        }

        [TestMethod]
        public void FakeNetworkControllerStartNetworking()
        {
            FakeNetworkController nc = new FakeNetworkController();
            nc.Start();

            Assert.IsTrue(FakeNetworkController.started);
        }

        [TestMethod]
        public void FakeNetworkControllerSendShutDownMessage()
        {
            FakeNetworkController nc = new FakeNetworkController();
            Networking.NetworkController = nc;
            Networking.Start();

            Stop.Ella();

            Assert.IsTrue(FakeNetworkController.shutDownMsgSent);
        }

        [TestMethod]
        public void NetworkControllerSubscribeTo()
        {
            byte[] b = new byte[1024];

            NetworkController nc = new NetworkController();
            FakeServer server = new FakeServer();

            nc.Servers.Add(server);
            Networking.NetworkController = nc;
            Networking.Start();

            FakeSender sender = new FakeSender();
            SenderBase.FactoryMethod = e => sender;

            Message msg = new Message();
            msg.Data = b;
            msg.Sender = EllaConfiguration.Instance.NodeId + 1;
            msg.Type = MessageType.Discover;

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("234.234.234.4"), 3456);

            server.DiscoveryMessageEvent(msg, ep);

            TestPublisher p = new TestPublisher();
            Start.Publisher(p);

            TestSubscriber s = new TestSubscriber();
            s.Subscribe();

            TestSubscriber s2 = new TestSubscriber();
            s2.Subscribe();

            Thread.Sleep(1000);

            Assert.AreEqual(2, sender._messages[MessageType.Subscribe]);
        }

        [TestMethod]
        public void NetworkControllerUnsubscribeFrom()
        {
            byte[] b = new byte[1024];

            NetworkController nc = new NetworkController();
            FakeServer fs = new FakeServer();
            FakeSender sender = new FakeSender();

            nc.Servers.Add(fs);
            Networking.NetworkController = nc;
            Networking.Start();

            SenderBase.FactoryMethod = e => sender;

            //this discoveryMessage is required to add the Instance to the RemoteHosts,
            //which is used in the NetworkController to send messages, subscribe, unsubscribe.. 
            //must call these MessageProcessor methods by hand, because the FakeSender just fakes the Send() implementation
            Message msg = new Message();
            msg.Data = b;
            msg.Sender = EllaConfiguration.Instance.NodeId + 1;
            msg.Type = MessageType.Discover;

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("234.234.234.4"), 3456);

            fs.DiscoveryMessageEvent(msg, ep);

            TestSubscriber s = new TestSubscriber();
            s.Subscribe();

            Thread.Sleep(1000);

            //after subscription is done: new MessageEvent of Type SubscribeResponse is faked in FakeServer
            fs.SubscribeResponseMessageEvent(sender._id);

            s.UnsubscribeFromRemote();

            Thread.Sleep(1000);

            Assert.AreEqual(1, sender._messages[MessageType.Unsubscribe]);
        }

        [TestMethod]
        public void NetworkControllerSendShutDownMessage()
        {
            byte[] b = new byte[1024];

            NetworkController nc = new NetworkController();
            FakeServer server = new FakeServer();
            FakeSender sender = new FakeSender();

            nc.Servers.Add(server);
            Networking.NetworkController = nc;
            Networking.Start();

            SenderBase.FactoryMethod = e => sender;

            Message msg = new Message();
            msg.Data = b;
            msg.Sender = EllaConfiguration.Instance.NodeId + 1;
            msg.Type = MessageType.Discover;

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("234.234.234.4"), 3456);

            server.DiscoveryMessageEvent(msg, ep);

            Stop.Ella();

            Assert.AreEqual(1, sender._messages[MessageType.NodeShutdown]);
        }

        [TestMethod]
        public void NetworkControllerStart()
        {
            NetworkController nc = new NetworkController();
            FakeServer server = new FakeServer();
            FakeSender sender = new FakeSender();

            nc.Servers.Add(server);
            Networking.NetworkController = nc;
            Networking.Start();

            Assert.IsTrue(server._startedNetwork);
        }

        [TestMethod]
        public void NetworkControllerConnectToMulticastGroup()
        {

            NetworkController nc = new NetworkController();
            FakeServer server = new FakeServer();
            FakeSender sender = new FakeSender();

            nc.Servers.Add(server);
            Networking.NetworkController = nc;
            Networking.Start();

            SenderBase.FactoryMethod = e => sender;

            MulticastRemoteSubscriptionhandle h = new MulticastRemoteSubscriptionhandle();

            Networking.ConnectToMulticast(h.IpAddress, h.Port);

            TestSubscriber s = new TestSubscriber();
            s.Subscribe();

            Assert.IsTrue(server._connectedToMulticastGroup);
        }

        [TestMethod]
        public void NetworkControllerSendMessage()
        {
            byte[] data = new byte[1024];

            NetworkController nc = new NetworkController();
            FakeServer server = new FakeServer();
            FakeSender sender = new FakeSender();

            nc.Servers.Add(server);
            Networking.NetworkController = nc;
            Networking.Start();

            SenderBase.FactoryMethod = e => sender;

            Message msg = new Message();
            msg.Data = data;
            msg.Sender = EllaConfiguration.Instance.NodeId + 1;
            msg.Type = MessageType.Discover;

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("234.234.234.4"), 3456);

            server.DiscoveryMessageEvent(msg, ep);

            TestSubscriber s = new TestSubscriber();
            s.Subscribe();

            byte[] b = { 1, 2 };

            ApplicationMessage app = new ApplicationMessage();
            app.Data = b;

            RemoteSubscriptionHandle rh = new RemoteSubscriptionHandle();
            rh.PublisherNodeID = EllaConfiguration.Instance.NodeId + 1;

            Send.Message(app, rh, s);

            Thread.Sleep(1000);

            Assert.AreEqual(1, sender._messages[MessageType.ApplicationMessage]);

        }

        [TestMethod]
        public void NetworkControllerSubscriptionMessageIsProcessed()
        {
            byte[] b = new byte[1024];

            NetworkController nc = new NetworkController();
            FakeServer server = new FakeServer();

            nc.Servers.Add(server);
            Networking.NetworkController = nc;
            Networking.Start();

            FakeSender sender = new FakeSender();
            SenderBase.FactoryMethod = e => sender;

            Message msg = new Message();
            msg.Data = b;
            msg.Sender = EllaConfiguration.Instance.NodeId + 1;
            msg.Type = MessageType.Discover;

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("234.234.234.4"), 3456);

            server.DiscoveryMessageEvent(msg, ep);

            PublisherWithCallbackMethod p = new PublisherWithCallbackMethod();
            Start.Publisher(p);


            server.SubscriptionMessage(typeof(bool));
            Thread.Sleep(1000);
            Assert.IsTrue(sender._messages.ContainsKey(MessageType.SubscribeResponse));
        }
    }
}
