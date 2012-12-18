using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ella;
using Ella.Attributes;
using Ella.Data;

namespace Ella
{
    [Publishes(typeof(String), 1)]
    [Publishes(typeof(String), 2)]
    public class TestPublisher
    {
        [Factory]
        public TestPublisher() { }

        [Start]
        public void Run() { }

        [Stop]
        public void Stop() { }

        [TemplateData(1)]
        [TemplateData(2)]
        public string GetTemplateObject(int id)
        {
            return id == 1 ? "hello" : string.Empty;
        }

        internal void PublishEvent()
        {
            Publish.PublishEvent("hello", this, 1);
        }
    }

    [Publishes(typeof(string), 1)]
    [Publishes(typeof(int), 1)]
    [Publishes(typeof(string), 2)]
    public class TestPublisherNonUniqueEventID
    {

    }

    [Publishes(typeof(int), 1)]
    public class TestPublisherPropertyTemplate
    {
        [Factory]
        public TestPublisherPropertyTemplate() { }

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

 
    [Publishes(typeof(String), 1, CopyPolicy = DataCopyPolicy.Copy)]
    public class CopyPolicyTrue
    {
        internal String data="hello";

         [Factory]
        public CopyPolicyTrue() { }

        [Start]
        public void Run() { }

        [Stop]
        public void Stop() { }

        internal void PublishEvent()
        {
            Publish.PublishEvent(data, this, 1);
        }
    }

    [Publishes(typeof(string),1,CopyPolicy = DataCopyPolicy.None)]
    public class CopyPolicyFalse
    {
         internal String data="hello";

         [Factory]
        public CopyPolicyFalse() { }

        [Start]
        public void Run() { }

        [Stop]
        public void Stop() { }

        internal void PublishEvent()
        {
            Publish.PublishEvent(data, this, 1);
        }
    }
}
