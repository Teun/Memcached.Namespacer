using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Enyim.Caching;
using MemcachedMock;
using Enyim.Caching.Memcached;

namespace CacheNamespacer.Tests
{
    [TestClass]
    public class Expiration : BaseTest
    {
        [TestMethod]
        public void WhenCacheClearedTwice_StorageDissappearsTwice()
        {
            init();

            string key = ns.GetNamespaced("userId", 1234);
            cache.Store(StoreMode.Set, key, "test");
            string found = cache.Get<String>(ns.GetNamespaced("userId", 1234));
            Assert.IsNotNull(found);

            ns.ClearCache("userId", 1234);
            key = ns.GetNamespaced("userId", 1234);
            found = cache.Get<String>(ns.GetNamespaced("userId", 1234));
            Assert.IsNull(found);

            key = ns.GetNamespaced("userId", 1234);
            cache.Store(StoreMode.Set, key, "test");
            found = cache.Get<String>(ns.GetNamespaced("userId", 1234));
            Assert.IsNotNull(found);

            ns.ClearCache("userId", 1234);
            key = ns.GetNamespaced("userId", 1234);
            found = cache.Get<String>(ns.GetNamespaced("userId", 1234));
            Assert.IsNull(found);
        }
        [TestMethod]
        public void DefaultCounterWorksAfterLongTime()
        {
            init();
            ITime time = ((ICacheMeta)cache).Time;

            string key = ns.GetNamespaced("userId", 1234);
            cache.Store(StoreMode.Set, key, "test", TimeSpan.FromHours(2));
            string found = cache.Get<String>(ns.GetNamespaced("userId", 1234));
            Assert.IsNotNull(found);

            time.Proceed(TimeSpan.FromHours(1));
            key = ns.GetNamespaced("userId", 1234);
            found = cache.Get<String>(key);
            Assert.IsNotNull(found);

            time.Proceed(TimeSpan.FromDays(2));
            string newkey = ns.GetNamespaced("userId", 1234);
            found = cache.Get<String>(newkey);
            Assert.IsNull(found);
            Assert.AreEqual(key, newkey);
        }
    }
}
