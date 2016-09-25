using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Enyim.Caching;
using MemcachedMock;
using Enyim.Caching.Memcached;

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
            ns.FlushAllRolling();
            this.Elapse(TimeSpan.FromHours(2));
            string fromCache = cache.Get<string>(ns.GetNamespaced("test"));
            Assert.IsNull(fromCache);
        }
    }
}
