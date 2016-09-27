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
        private NamespacerOptions _opt;

        protected void init()
        {
            initWithOptions(null);
        }
        protected void initWithOptions(NamespacerOptions opt)
        {
            cache = new CacheMock();
            _opt = opt;
            setContext(new DateTime(2016, 3, 1, 12, 0, 0));
        }

        private void setContext(DateTime newTime)
        {
            time = ((ICacheMeta)cache).Time;
            time.Set(newTime);
            ns = new TimeFakableNamespacer(cache, _opt);
            ns.SetTime(time.Now());
        }
        protected void ResetContext()
        {
            setContext(time.Now());
        }
        protected void Elapse(TimeSpan ts)
        {
            time.Proceed(ts);
            ns.SetTime(time.Now());
        }

    }
}