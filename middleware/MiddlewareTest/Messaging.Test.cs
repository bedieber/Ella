using System.Net;
using System.Threading;
using Ella.Controller;
using Ella.Fakes;
using Ella.Internal;
using Ella.Network;
using Ella.Network.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ella
{
    [TestClass]
    public class Messaging
    {
        [TestMethod]
        public void SendLocalApplicationMessageSubscriberToPublisher()
        {
            TestPublisher tp = new TestPublisher();
            Start.Publisher(tp);
            TestSubscriber s = new TestSubscriber();
            s.Subscribe();
            s.SendMessage();
            Thread.Sleep(500);
            Assert.IsTrue(tp.MessageReceived);
        }
        [TestMethod]
        public void SendLocalApplicationMessagePublisherToSubscriber()
        {
            PublisherWithCallbackMethod tp = new PublisherWithCallbackMethod();
            Start.Publisher(tp);
            TestSubscriber s = new TestSubscriber();
            s.SubscribeToBool();
            tp.SendMessage();
            Thread.Sleep(500);
            Assert.IsTrue(s.ReplyReceived);
        }
        //TODO tests for: Not subscribed
        [TestMethod]
        public void ReplyToApplicationMessage()
        {
            TestPublisher tp = new TestPublisher();
            Start.Publisher(tp);
            TestSubscriber s = new TestSubscriber();
            s.Subscribe();
            s.SendMessage();
            Thread.Sleep(500);
            Assert.IsTrue(s.ReplyReceived);
        }

        [TestMethod]
        public void ReplyToApplicationMessagePublisherToSubscriber()
        {
            PublisherWithCallbackMethod tp = new PublisherWithCallbackMethod();
            Start.Publisher(tp);
            TestSubscriber s = new TestSubscriber();
            s.SubscribeToBool();
            tp.SendMessage();
            Thread.Sleep(500);
            Assert.IsTrue(tp._replyReceived);
        }

        [TestMethod]
        public void MessageIsSentToNetworkController()
        {
            FakeNetworkController nc = new FakeNetworkController();
            Networking.NetworkController = nc;
            Networking.Start();

            TestSubscriber s = new TestSubscriber();
            s.Subscribe();

            s.SendMessage();

            Assert.AreEqual(1, nc.MessageTypes[1]);
        }

        [TestMethod]
        public void MessageFromSubscriberIsSentToCorrectHost()
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

            TestSubscriber s = new TestSubscriber();
            s.Subscribe();
            server.SubscribeResponseMessageEvent(sender._id);
            Thread.Sleep(1000);
            s.SendMessage();
            Thread.Sleep(1000);
            Assert.AreEqual(1, sender._messages[MessageType.ApplicationMessage]);
        }

        [TestMethod]
        public void ReplyIsSentFromPublisherToSubscriber()
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

            PublisherWithCallbackMethod p=new PublisherWithCallbackMethod();
            Start.Publisher(p);

            server.SubscriptionMessage(typeof(bool));
            Thread.Sleep(1000);
            p.SendMessageReply();
            Thread.Sleep(1000);
            Assert.IsTrue(sender._messages.ContainsKey(MessageType.ApplicationMessageResponse));
        }

        [TestMethod]
        public void ReplyIsSentFromSubscriberToPublisher()
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

            TestSubscriber s = new TestSubscriber();
            s.Subscribe();

            server.SubscribeResponseMessageEvent(sender._id);
            Thread.Sleep(1000);
            s.SendMessageReply();
            Thread.Sleep(1000);
            Assert.IsTrue(sender._messages.ContainsKey(MessageType.ApplicationMessageResponse));
        }
    }
}
