using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Enyim.Caching;
using MemcachedMock;
using Enyim.Caching.Memcached;

namespace CacheNamespacer.Tests
{
    [TestClass]
    public class Optimized : BaseTest
    {
        [TestMethod]
        public void NamespaceChanges_DontAffectOtherKeys()
        {
            initWithOptions(new NamespacerOptions { EvidenceSize = 8 });

            string key1 = ns.GetNamespaced("userId", 1234);
            cache.Store(StoreMode.Set, key1, "test");
            for (int i = 2000; i < 3000; i++)
            {
                ns.UpdateNamespace("userId", i);
            }

            var stored = cache.Get<string>(ns.GetNamespaced("userId", 1234));
            Assert.AreEqual("test", stored);
        }
        [TestMethod]
        public void FalsePositives_StillWork()
        {
            initWithOptions(new NamespacerOptions { EvidenceSize = 8 });

            string key1 = ns.GetNamespaced("userId", 1234);
            this.Elapse(TimeSpan.FromMinutes(5));
            ns.UpdateNamespace("userId", 1234);

            for (int i = 2000; i < 30000; i++)
            {
                string key = ns.GetNamespaced("userId", i);
                Assert.IsTrue(key.EndsWith(key1.Substring(key1.Length - 8)));
            }
        }
    }
}
