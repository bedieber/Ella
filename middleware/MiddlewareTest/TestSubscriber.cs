using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ella.Attributes;
using Ella;
using Ella.Data;

namespace Ella
{
    [Subscriber]
    public class TestSubscriber
    {
        internal String rec = "";
        internal int numEventsReceived = 0;
        [Factory]
        public TestSubscriber() { }

        internal void Subscribe()
        {
            Ella.Subscribe.To<string>(this, Callback);
        }

        private void Callback(string s)
        {
            rec = s;
            if (s == "hello")
                numEventsReceived++;
        }

        internal void SubscribeWithObject()
        {
            Ella.Subscribe.To<String>(this, Callback, evaluateTemplateObject: EvaluateTemplateObject);
        }

        private bool EvaluateTemplateObject(string s)
        {
            return s == "hello";
        }

        internal void SubscribeWithModifyTrue()
        {
            Ella.Subscribe.To<String>(this, Callback, DataModifyPolicy.Modify);
        }

        internal void SubscribeWithModifyFalse()
        {
            Ella.Subscribe.To<String>(this, Callback, DataModifyPolicy.NoModify);
        }
    }

    [Subscriber]
    public class TestSubscriberMethodFactory
    {
        [Factory]
        public TestSubscriberMethodFactory CreateInstance() { return new TestSubscriberMethodFactory(); }
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
        static TestSubscriberStaticConstructerFactory() { }
    }


}
