using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Middleware.Attributes;

namespace MiddlewareTest
{
    [Publishes(typeof(String),1)]
    public class TestPublisher
    {
        [Factory]
        public TestPublisher(){}
    }
}
