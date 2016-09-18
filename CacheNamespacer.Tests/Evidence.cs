using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Enyim.Caching;
using MemcachedMock;
using System.Linq;

namespace CacheNamespacer.Tests
{
    [TestClass]
    public class EvidenceTests
    {
        [TestMethod]
        public void EmptyEvidenceRecognizesNothing()
        {
            var ev = new Evidence(1, 80);


            Assert.IsTrue(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 1001, 1002, 1003 }.All(i => ev.For(i) == false));

        }
        [TestMethod]
        public void EvidenceIsFound()
        {
            var ev = new Evidence(1, 80);

            ev.Witness(10);
            Assert.IsTrue(ev.For(10));

        }
        [TestMethod]
        public void EvidenceIsFoundLarge()
        {
            var ev = new Evidence(1, 80);

            ev.Witness(10000);
            Assert.IsTrue(ev.For(10000));
        }
        [TestMethod]
        public void EvidenceIsNotFound_ForModuloLength()
        {
            var ev = new Evidence(1, 80);

            ev.Witness(650); //640 + 10
            Assert.IsFalse(ev.For(10));
        }
        [TestMethod]
        public void EvidenceHasFalsePositives()
        {
            var ev = new Evidence(1, 80);
            for (int i = 1000; i < 1100; i++)
            {
                ev.Witness(i); 
            }
            // in theory, with 640 bits of evidence and 100 random touched values and 2 independent bits per touch,
            // we expect a p(false positive) of around 7.2%. In reality, it will be slightly higher (because of no 
            // half bits, because we don't use all bit as well)
            Console.WriteLine(Enumerable.Range(10000, 10000).Count(i => ev.For(i)));
            Assert.IsTrue(Enumerable.Range(10000, 10000).Count(i => ev.For(i)) > 500);
        }
    }
}
