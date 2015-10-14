using LevelDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IceWallet.Implementations.Blockchains.LevelDB
{
    internal static class Helper
    {
        public static IEnumerable<T> Find<T>(this DB db, ReadOptions options, Slice prefix, Func<Slice, Slice, T> resultSelector)
        {
            using (Iterator it = db.NewIterator(options))
            {
                for (it.Seek(prefix); it.Valid(); it.Next())
                {
                    Slice key = it.Key();
                    byte[] x = key.ToArray();
                    byte[] y = prefix.ToArray();
                    if (x.Length < y.Length) break;
                    if (!x.Take(y.Length).SequenceEqual(y)) break;
                    yield return resultSelector(key, it.Value());
                }
            }
        }

        public static uint[] GetUInt32Array(this byte[] source)
        {
            if (source == null) throw new ArgumentNullException();
            int rem;
            int size = Math.DivRem(source.Length, sizeof(uint), out rem);
            if (rem != 0) throw new ArgumentException();
            uint[] dst = new uint[size];
            Buffer.BlockCopy(source, 0, dst, 0, source.Length);
            return dst;
        }

        public static byte[] ToByteArray(this IEnumerable<uint> source)
        {
            uint[] src = source.ToArray();
            byte[] dst = new byte[src.Length * sizeof(uint)];
            Buffer.BlockCopy(src, 0, dst, 0, dst.Length);
            return dst;
        }
    }
}
