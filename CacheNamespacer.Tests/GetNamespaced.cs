using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Enyim.Caching;
using MemcachedMock;

namespace CacheNamespacer.Tests
{
    [TestClass]
    public class GetNamespaced : BaseTest
    {
        [TestMethod]
        public void WhenQueriedMultipleTimes_NamespaceRemainsSame()
        {
            init();

            string key1 = ns.GetNamespaced("userId", 1234);

            string key2 = ns.GetNamespaced("userId", 1234);

            Assert.AreEqual(key1, key2);
        }
        [TestMethod]
        public void WhenQueriedMultipleTimes_NamespaceRemainsSame_Simple()
        {
            init();

            string key1 = ns.GetNamespaced("ranking");

            string key2 = ns.GetNamespaced("ranking");

            Assert.AreEqual(key1, key2);
        }
        [TestMethod]
        public void WhenCacheCleared_NamespaceNotSame()
        {
            init();

            string key1 = ns.GetNamespaced("userId", 1234);
            ns.ClearCache("userId", 1234);

            string key2 = ns.GetNamespaced("userId", 1234);

            Assert.AreNotEqual(key1, key2);
        }
        [TestMethod]
        public void WhenCacheCleared_NamespaceNotSame_Simple()
        {
            init();

            string key1 = ns.GetNamespaced("leaderBoard");
            ns.ClearCache("leaderBoard");

            string key2 = ns.GetNamespaced("leaderBoard");

            Assert.AreNotEqual(key1, key2);
        }
        [TestMethod]
        public void WhenCacheClearedForOtherEntry_NamespaceRemainsSame()
        {
            init();

            string key1 = ns.GetNamespaced("userId", 1234);
            ns.ClearCache("schoolId", 1234);
            ns.ClearCache("userId", 1235);

            string key2 = ns.GetNamespaced("userId", 1234);

            Assert.AreEqual(key1, key2);
        }
        [TestMethod]
        public void ByDefaultAllKeysStartWith___()
        {
            init();

            string key1 = ns.GetNamespaced("userId", 1234);
            Assert.IsTrue(key1.StartsWith("___"));
        }
        [TestMethod]
        public void PrefixConBeSetThroughOptions()
        {
            initWithOptions(new NamespacerOptions() { Prefix = "bla" });

            string key1 = ns.GetNamespaced("userId", 1234);
            Assert.IsTrue(key1.StartsWith("bla"));
        }
    }
}
