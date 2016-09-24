using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheNamespacer
{
    public class Evidence
    {
        BitArray bits;
        uint defaultCounter;
        uint prime;
        int offset;

        public uint DefaultCounter { get { return defaultCounter; } }

        public Evidence(uint seed, int size)
        {
            defaultCounter = seed;
            bits = new BitArray(size*8, false);
            prime = somePrimes.Last(p => p < bits.Length);
            offset = bits.Length / 2;
        }
        public Evidence(byte[] data)
        {
            defaultCounter = BitConverter.ToUInt32(data, 0);
            bits = new BitArray(data.Skip(4).ToArray());
            prime = somePrimes.Last(p => p < bits.Length);
            offset = bits.Length / 2;
        }

        public byte[] ToBytes()
        {
            byte[] res = new byte[bits.Length + 4];
            Array.Copy(BitConverter.GetBytes(defaultCounter), res, 4);
            bits.CopyTo(res, 4);
            return res;
        }
        public float Quality
        {
            get
            {
                int empty = bits.OfType<bool>().Count(b => !b);
                float chanceFpPerCheck = 1 - (float)empty / bits.Count;
                return 1 - chanceFpPerCheck*chanceFpPerCheck;
            }
        }

        public void Witness(int value)
        {
            int posVal = Math.Abs(value);
            bits[posVal % (bits.Length)] = true;
            bits[(posVal + offset) % (int)prime] = true;
        }

        public bool For(int value)
        {
            int posVal = Math.Abs(value);
            if (bits[posVal % (bits.Length)]==true
                && bits[(posVal + offset) % (int)prime] == true
                ) { return true; }
            return false;
        }

        static uint[] somePrimes = new uint[] { 29,  71 , 113 , 173 , 229 , 281 , 349 , 409 , 463 , 541 , 601 , 659 , 733 , 809 , 863 ,
                                         941 ,1013 ,1069 ,1151 ,1223 ,1291 ,1373 ,1451 ,1511 ,1583 ,1657 ,1733 ,1811 ,1889 ,1987 ,2053 ,
                                        2129 ,2213 ,2287 ,2357 ,2423 ,2531 ,2617 ,2687 ,2741 ,2819 ,2903 ,2999 ,3079 ,3181 ,3257 ,3331 ,
                                        3413 ,3511 ,3571 ,3643 ,3727 ,3821 ,3907 ,3989 };

    }
}
