using Enyim.Caching;
using Enyim.Caching.Memcached;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheNamespacer
{
    public interface INamespacer
    {
        string GetNamespaced(string field, int value);
        void ClearCache(string field, int value);
        void ClearCache(string field);
        void ClearCacheRolling();
    }
    public class Namespacer : INamespacer
    {
        IMemcachedClient _cache;
        NamespacerOptions _opt = new NamespacerOptions();
        public Namespacer(IMemcachedClient cache, NamespacerOptions options = null) 
        {
            _cache = cache;
            if(options!=null) _opt = options;
        }

        public void ClearCache(string field)
        {
            throw new NotImplementedException();
        }

        public void ClearCache(string field, int value)
        {
            string storeKey = counterStoreKey(field, value);
            _cache.Increment(storeKey, 1, 1);
        }

        public void ClearCacheRolling()
        {
            throw new NotImplementedException();
        }

        public string GetNamespaced(string field, int value)
        {
            uint counter = getCurrentCounter(field, value);
            return namespacedKey(field, value, counter);

        }

        private uint getCurrentCounter(string field, int value)
        {
            if (_opt.OptimizeWithDefaultCounterAndEvidence)
            {
                return optimizedGetCurrentCounter(field, value);
            }
            else
            {
                uint start = getCounterStart();
                return simpleGetCurrentCounter(field, value, start);
            }
        }

        private uint optimizedGetCurrentCounter(string field, int value)
        {
            string evidenceKey = getEvidenceKey();
            byte[] evidenceData = _cache.Get<byte[]>(evidenceKey);
            Evidence evidence;
            if(evidenceData == null)
            {
                evidence = new Evidence(getCounterStart(), _opt.EvidenceSize);
            }
            else
            {
                evidence = new Evidence(evidenceData);
            }
            if (!evidence.For(value))
            {
                return evidence.DefaultCounter;
            }
            // check evidence with value
            // return default or return current counter
            return simpleGetCurrentCounter(field, value, getCounterStart());
        }

        private uint simpleGetCurrentCounter(string field, int value, uint startCounter)
        {
            string storeKey = counterStoreKey(field, value);
            object counter;
            if (!_cache.TryGet(storeKey, out counter))
            {
                counter = startCounter;
                _cache.Store(StoreMode.Set, storeKey, counter);
            }
            return Convert.ToUInt32(counter);
        }

        private uint getCounterStart()
        {
            return (uint)(DateTime.Now - DateTime.Today).TotalMilliseconds;
        }

        private string namespacedKey(string field, int value, object counter)
        {
            return String.Format("{0}v::{1}::{2}::{3}", _opt.Prefix, field, value, counter);
        }

        private string counterStoreKey(string field, int value)
        {
            return String.Format("{0}nss::{1}::{2}", _opt.Prefix, field, value);
        }

        private string getEvidenceKey()
        {
            return String.Format("{0}ev", _opt.Prefix);
        }
    }
}
