using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Middleware.Attributes;

namespace MiddlewareTest
{
    [Subscriber]
    public class TestSubscriber
    {
        [FactoryAttribute]
        public TestSubscriber(){}
    }

    [Subscriber]
    public class TestSubscriberMethodFactory
    {
        [FactoryAttribute]
        public TestSubscriberMethodFactory CreateInstance() {return new TestSubscriberMethodFactory(); }
    }

    [Subscriber]
    public class TestSubscriberStaticMethodFactory
    {
        [FactoryAttribute]
        public static TestSubscriberStaticMethodFactory CreateInstance() { return new TestSubscriberStaticMethodFactory(); }
    }

    [Subscriber]
    public class TestSubscriberNoFactory
    {
        
    }

    [Subscriber]
    public class TestSubscriberStaticConstructerFactory
    {
        [FactoryAttribute]
        static TestSubscriberStaticConstructerFactory () { }
    }


}
