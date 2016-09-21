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
        string GetNamespaced(string value);
        string GetNamespaced(string field, int value);
        void ClearCache(string field, int value);
        void ClearCache(string field);
        void ClearCacheRolling();
    }
    public class Namespacer : INamespacer
    {
        private const string UNCOMMONSEPARATOR = "ϯϯ"; // unicode 03ef = COPTIC SMALL LETTER DEI 
        IMemcachedClient _cache;
        NamespacerOptions _opt = new NamespacerOptions();
        public Namespacer(IMemcachedClient cache, NamespacerOptions options = null) 
        {
            _cache = cache;
            if(options!=null) _opt = options;
        }

        public void ClearCache(string value)
        {
            string storeKey = counterStoreKey(value);
            _cache.Increment(storeKey, 1, 1);
        }

        public void ClearCache(string field, int value)
        {
            string combined = combinedFieldAndValue(field, value);
            string storeKey = counterStoreKey(combined);
            _cache.Increment(storeKey, 1, 1);
        }

        public void ClearCacheRolling()
        {
            throw new NotImplementedException();
        }

        public string GetNamespaced(string field, int value)
        {
            string combined = combinedFieldAndValue(field, value);
            return GetNamespaced(combined);
        }

        public string GetNamespaced(string value)
        {
            uint counter = getCurrentCounter(value);
            return namespacedKey(value, counter);
        }

        private uint getCurrentCounter(string value)
        {
            if (_opt.OptimizeWithDefaultCounterAndEvidence)
            {
                return optimizedGetCurrentCounter(value);
            }
            else
            {
                uint start = getCounterStart();
                return simpleGetCurrentCounter(value, start);
            }
        }

        private uint optimizedGetCurrentCounter(string value)
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
            if (!evidence.For(GetStringHash(value)))
            {
                return evidence.DefaultCounter;
            }
            // check evidence with value
            // return default or return current counter
            return simpleGetCurrentCounter(value, getCounterStart());
        }

        // found this hashing function at http://stackoverflow.com/questions/9545619/a-fast-hash-function-for-string-in-c-sharp
        // TODO: see if we can have a more lightweight hash function (but not String.GetHashcode for compatibility between platforms)
        private int GetStringHash(string value)
        {
            UInt64 hashedValue = 3074457345618258791ul;
            for (int i = 0; i < value.Length; i++)
            {
                hashedValue += value[i];
                hashedValue *= 3074457345618258799ul;
            }
            return (int)hashedValue;
        }

        private uint simpleGetCurrentCounter(string value, uint startCounter)
        {
            string storeKey = counterStoreKey(value);
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

        private string namespacedKey(string value, object counter)
        {
            return String.Format("{1}v{0}{2}{0}{3}", UNCOMMONSEPARATOR, _opt.Prefix, value, counter);
        }

        private string counterStoreKey(string value)
        {
            return String.Format("{1}nss{0}{2}", UNCOMMONSEPARATOR, _opt.Prefix, value);
        }
        private string combinedFieldAndValue(string field, int value)
        {
            return field + UNCOMMONSEPARATOR + value.ToString();
        }


        private string getEvidenceKey()
        {
            return String.Format("{0}ev", _opt.Prefix);
        }

    }
}
