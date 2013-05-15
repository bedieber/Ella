using Ella.Controller;
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
            TestPublisher p = new TestPublisher();
            Start.Publisher(p);
            SubscriptionController.RemoteSubscriber(typeof (string), 1, null, 3);
            Assert.IsTrue(EllaModel.Instance.Subscriptions.Count == 2);
            Message m = new Message(3);
            NetworkController.ProcessUnsubscribe(new MessageEventArgs(m));
            Assert.IsTrue(EllaModel.Instance.Subscriptions.Count == 0);
        }

        [TestMethod]
        public void MulticastAddressIsInMulticastRange()
        {
            
        }

        [TestMethod]
        public void MulticastAddressIsNotInMulticastRange()
        {
            
        }

    }
}
