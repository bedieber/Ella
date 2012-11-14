using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MiddlewareTest
{
    [TestClass]
    public class TestMiddleware
    {
        [TestMethod]
        public void LoadPublishers()
        {
            Middleware.Middleware mw = new Middleware.Middleware();
            Assert.IsNotNull(mw.Publishers);
            FileInfo fi = new FileInfo(GetType().Assembly.Location);
            Assembly a = Assembly.LoadFile(fi.FullName);
            mw.LoadPublishers(a);
            Assert.IsTrue(mw.Publishers.Count() == 1);
        }
    }
}
