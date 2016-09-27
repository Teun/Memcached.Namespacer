using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Enyim.Caching;
using MemcachedMock;
using Enyim.Caching.Memcached;
using System.Linq;

namespace CacheNamespacer.Tests
{
    [TestClass]
    public class RollingFlush : BaseTest
    {
        [TestMethod]
        public void RollingFlushOnlyWorksWithOptimizationEnabled_Throw()
        {
            initWithOptions(new NamespacerOptions() { OptimizeWithDefaultCounterAndEvidence = false });
            bool ok = false;
            try {
                ns.FlushAllRolling();
            }
            catch (Exception)
            {
                ok = true;
            }
            Assert.IsTrue(ok);
        }
        [TestMethod]
        public void RollingFlushWorksWithOptimizationEnabled()
        {
            initWithOptions(new NamespacerOptions() { OptimizeWithDefaultCounterAndEvidence = true });

            cache.Store(StoreMode.Set, ns.GetNamespaced("test"), "value");
            this.Elapse(TimeSpan.FromSeconds(2));
            ns.FlushAllRolling();
            this.Elapse(TimeSpan.FromHours(2));
            string fromCache = cache.Get<string>(ns.GetNamespaced("test"));
            Assert.IsNull(fromCache);
        }
        [TestMethod]
        public void RollingFlushHalfway_ShouldHaveAboutHalfOfKeys()
        {
            initWithOptions(new NamespacerOptions() { OptimizeWithDefaultCounterAndEvidence = true, RollingPeriod = TimeSpan.FromSeconds(60) });
            for (int i = 0; i < 1000; i++)
            {
                cache.Store(StoreMode.Set, ns.GetNamespaced("test", i), i);
            }
            this.Elapse(TimeSpan.FromSeconds(30));
            ns.FlushAllRolling();
            this.Elapse(TimeSpan.FromSeconds(30));
            object v;
            int count = Enumerable.Range(0, 100).Count(i => cache.TryGet(ns.GetNamespaced("test", i), out v));

            Assert.IsTrue(count > 40 && count < 70);
        }
        [TestMethod]
        public void RollingFlush_ShouldInitiallyHaveAllKeys()
        {
            initWithOptions(new NamespacerOptions() { OptimizeWithDefaultCounterAndEvidence = true, RollingPeriod = TimeSpan.FromSeconds(60) });
            for (int i = 0; i < 1000; i++)
            {
                cache.Store(StoreMode.Set, ns.GetNamespaced("test", i), i);
            }
            this.Elapse(TimeSpan.FromSeconds(30));
            ns.FlushAllRolling();
            object v;
            int count = Enumerable.Range(0, 100).Count(i => cache.TryGet(ns.GetNamespaced("test", i), out v));

            Assert.AreEqual(count, 100);
        }
    }
}
