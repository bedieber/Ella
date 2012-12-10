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
            Ella.Subscribe.To<string>(this,Callback);
        }

        private void Callback(string s)
        {
            if (s == "hello")
                numEventsReceived++;
        }

        internal void SubscribeWithObject()
        {
           Ella.Subscribe.To<String>(this, Callback, EvaluateTemplateObject);
        }

        private bool EvaluateTemplateObject(string s)
        {
            return s == "hello";
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
