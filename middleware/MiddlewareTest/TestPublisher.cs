using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ella.Attributes;

namespace MiddlewareTest
{
    [Publishes(typeof(String),1)]
    public class TestPublisher
    {
        [Factory]
        public TestPublisher(){}

        [Start]
        public void Run(){}

        [Stop]
        public void Stop(){}
    }

    [Publishes(typeof(string),1)]
    [Publishes(typeof(int),1)]
    [Publishes(typeof(string),2)]
    public class TestPublisherNonUniqueEventID
    {
        
    }
}
