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
        public void ResetWhenEvidenceMuddled_Works()
        {
            int bytes = 8;
            initWithOptions(new NamespacerOptions { EvidenceSize = bytes, ResetWhenEvidenceMuddled=true });

            cache.Store(StoreMode.Set, ns.GetNamespaced("prod", 1), "bla");
            for (int i = 0; i < 1000; i++)
            {
                Elapse(TimeSpan.FromMilliseconds(1));
                ns.UpdateNamespace("user", i);
                if(ns.Evidence.Quality == 1)
                {
                    Console.WriteLine("Evidence got softly reset after {0} namepsace changes (using {1} bytes of evidence)", i, bytes);
                    break;
                }
            }
            Assert.AreEqual("bla", cache.Get(ns.GetNamespaced("prod", 1)));
            Elapse(TimeSpan.FromSeconds(200));
            Assert.AreNotEqual("bla", cache.Get(ns.GetNamespaced("prod", 1)));
        }
        [TestMethod]
        public void ResetWhenEvidenceMuddledSetTrue_WillEventuallyNotWorkAtAll()
        {
            initWithOptions(new NamespacerOptions { ResetWhenEvidenceMuddled = false });

            cache.Store(StoreMode.Set, ns.GetNamespaced("prod", 1), "bla");
            for (int i = 0; i < 1000; i++)
            {
                Elapse(TimeSpan.FromMilliseconds(1));
                ns.UpdateNamespace("user", i);
            }
            Assert.AreEqual("bla", cache.Get(ns.GetNamespaced("prod", 1)));
            Elapse(TimeSpan.FromSeconds(200));
            Assert.AreEqual("bla", cache.Get(ns.GetNamespaced("prod", 1)));
            Assert.IsTrue(ns.Evidence.Quality < 0.1);
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
