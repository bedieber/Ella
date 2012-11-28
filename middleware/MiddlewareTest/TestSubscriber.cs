using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ella.Attributes;
using Ella;

namespace Ella
{
    [Subscriber]
    public class TestSubscriber
    {
        internal int numEventsReceived = 0;
        [Factory]
        public TestSubscriber(){}

        internal void Subscribe()
        {
            //TODO subscribe to event, provide handler, count numEventsReceived up if event data =="hello"
            Ella.Subscribe.To(typeof(string), this);
        }
    }

    [Subscriber]
    public class TestSubscriberMethodFactory
    {
        [Factory]
        public TestSubscriberMethodFactory CreateInstance() {return new TestSubscriberMethodFactory(); }
    }

    [Subscriber]
    public class TestSubscriberStaticMethodFactory
    {
        [Factory]
        public static TestSubscriberStaticMethodFactory CreateInstance() { return new TestSubscriberStaticMethodFactory(); }
    }

    [Subscriber]
    public class TestSubscriberNoFactory
    {
        
    }

    [Subscriber]
    public class TestSubscriberStaticConstructerFactory
    {
        [Factory]
        static TestSubscriberStaticConstructerFactory () { }
    }


}
