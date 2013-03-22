using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ella
{
    [TestClass]
    public class Messaging
    {
        [TestMethod]
        public void SendLocalApplicationMessage()
        {
            TestPublisher tp = new TestPublisher();
            Start.Publisher(tp);
            TestSubscriber s = new TestSubscriber();
            s.Subscribe();
            s.SendMessage();
            Thread.Sleep(500);
            Assert.IsTrue(tp.MessageReceived);
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
    }
}
