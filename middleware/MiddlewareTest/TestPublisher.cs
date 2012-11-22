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

        [TemplateData(1)]
        public string GetTemplateObject()
        {
            return string.Empty;
        }
    }

    [Publishes(typeof(string),1)]
    [Publishes(typeof(int),1)]
    [Publishes(typeof(string),2)]
    public class TestPublisherNonUniqueEventID
    {
        
    }

    [Publishes(typeof(int), 1)]
    public class TestPublisherPropertyTemplate
    {
        [Factory]
        public TestPublisherPropertyTemplate(){}

        [Start]
        public void Run() { }

        [Stop]
        public void Stop() { }

        [TemplateData(1)]
        public int TemplateObject
        {
            get { return 0; }
        }
    }

    [Publishes(typeof(int), 1)]
    [Publishes(typeof(string), 2)]
    public class TestPublisherMultipleEvents
    {
        [Factory]
        public TestPublisherMultipleEvents() { }

        [Start]
        public void Run() { }

        [Stop]
        public void Stop() { }

        [TemplateData(1)]
        public int TemplateObject
        {
            get { return 0; }
        }
        [TemplateData(2)]
        public string GetTemplateObject()
        {
            return "hallo";
        }
    }
}
