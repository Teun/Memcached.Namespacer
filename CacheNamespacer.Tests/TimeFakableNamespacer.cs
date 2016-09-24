using Enyim.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheNamespacer.Tests
{
    public class TimeFakableNamespacer : Namespacer
    {
        public TimeFakableNamespacer(IMemcachedClient cache, NamespacerOptions options = null)
            : base(cache, options)
        { }

        private DateTime? _staticTime = null;
        protected override DateTime Now()
        {
            if (_staticTime.HasValue)
            {
                return _staticTime.Value;
            }
            return base.Now();
        }
        public void SetTime(DateTime dt) { _staticTime = dt; }

    }
}
