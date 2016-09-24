using Enyim.Caching;
using MemcachedMock;
using System;

namespace CacheNamespacer.Tests
{
    public class BaseTest
    {
        protected IMemcachedClient cache;
        protected TimeFakableNamespacer ns;
        protected ITime time;

        protected void init()
        {
            initWithOptions(null);
        }
        protected void initWithOptions(NamespacerOptions opt)
        {
            cache = new CacheMock();
            time = ((ICacheMeta)cache).Time;
            time.Set(new DateTime(2016, 1, 1, 12, 0, 0));
            ns = new TimeFakableNamespacer(cache, opt);
            ns.SetTime(time.Now());

        }

        protected void Elapse(TimeSpan ts)
        {
            time.Proceed(ts);
            ns.SetTime(time.Now());
        }

    }
}