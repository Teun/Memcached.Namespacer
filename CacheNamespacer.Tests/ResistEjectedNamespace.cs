using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Enyim.Caching;
using MemcachedMock;

namespace CacheNamespacer.Tests
{
    [TestClass]
    public class ResistEjectedNamespace : BaseTest
    {
        [TestMethod]
        public void CounterStoreEject_ShouldNotRevertToOldContent()
        {
            init();

            string key1 = ns.GetNamespaced("userId", 1234);
            ns.ClearCache("userId", 1234);
            string key2 = ns.GetNamespaced("userId", 1234);

            // now for some reason, the counter storage for this namespace gets ejected:
            cache.FlushAll();
            string key3 = ns.GetNamespaced("userId", 1234);
            Assert.AreNotEqual(key1, key3);
        }
    }
}
