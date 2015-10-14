using IceWallet.IO;
using System;
using System.IO;

namespace IceWallet
{
    /// <summary>
    /// 精确到10^-8的64位定点数，将舍入误差降到最低。
    /// 通过控制乘数的精度，可以完全消除舍入误差。
    /// </summary>
    public struct Fixed8 : IComparable<Fixed8>, IEquatable<Fixed8>, IFormattable, ISerializable
    {
        private const ulong D = 100000000;
        internal ulong value;

        public static readonly Fixed8 MaxValue = new Fixed8 { value = ulong.MaxValue };
        public static readonly Fixed8 MinValue = new Fixed8 { value = ulong.MinValue };
        public static readonly Fixed8 One = new Fixed8 { value = D };
        public static readonly Fixed8 Satoshi = new Fixed8 { value = 1 };
        public static readonly Fixed8 Zero = new Fixed8();

        public Fixed8(ulong data)
        {
            this.value = data;
        }

        public int CompareTo(Fixed8 other)
        {
            return value.CompareTo(other.value);
        }

        void ISerializable.Deserialize(BinaryReader reader)
        {
            this.value = reader.ReadUInt64();
        }

        public bool Equals(Fixed8 other)
        {
            return value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Fixed8)) return false;
            return Equals((Fixed8)obj);
        }

        public static Fixed8 FromDecimal(decimal value)
        {
            value *= D;
            if (value < ulong.MinValue || value > ulong.MaxValue)
                throw new OverflowException();
            return new Fixed8
            {
                value = (ulong)value
            };
        }

        public ulong GetData() => value;

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public static Fixed8 Parse(string s)
        {
            return FromDecimal(decimal.Parse(s));
        }

        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.Write(value);
        }

        public decimal ToDecimal()
        {
            return value / (decimal)D;
        }

        public override string ToString()
        {
            return ToDecimal().ToString();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToDecimal().ToString(format, formatProvider);
        }

        public static bool TryParse(string s, out Fixed8 result)
        {
            decimal d;
            if (!decimal.TryParse(s, out d))
            {
                result = default(Fixed8);
                return false;
            }
            d *= D;
            if (d < ulong.MinValue || d > ulong.MaxValue)
            {
                result = default(Fixed8);
                return false;
            }
            result = new Fixed8
            {
                value = (ulong)d
            };
            return true;
        }

        public static bool operator ==(Fixed8 x, Fixed8 y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Fixed8 x, Fixed8 y)
        {
            return !x.Equals(y);
        }

        public static bool operator >(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) > 0;
        }

        public static bool operator <(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) < 0;
        }

        public static bool operator >=(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) >= 0;
        }

        public static bool operator <=(Fixed8 x, Fixed8 y)
        {
            return x.CompareTo(y) <= 0;
        }

        public static Fixed8 operator *(Fixed8 x, Fixed8 y)
        {
            const ulong QUO = (1ul << 63) / (D >> 1);
            const ulong REM = (1ul << 63) % (D >> 1);
            ulong xh = x.value >> 32;
            ulong xl = x.value & 0x00000000fffffffful;
            ulong yh = y.value >> 32;
            ulong yl = y.value & 0x00000000fffffffful;
            ulong rh = xh * yh;
            ulong rm = xh * yl + xl * yh;
            ulong rl = xl * yl;
            ulong rmh = rm >> 32;
            ulong rml = rm << 32;
            rh += rmh;
            rl += rml;
            if (rl < rml)
                ++rh;
            if (rh >= D)
                throw new OverflowException();
            x.value = rh * QUO + (rh * REM + rl) / D;
            return x;
        }

        public static Fixed8 operator /(Fixed8 x, ulong y)
        {
            x.value /= y;
            return x;
        }

        public static Fixed8 operator +(Fixed8 x, Fixed8 y)
        {
            x.value = checked(x.value + y.value);
            return x;
        }

        public static Fixed8 operator -(Fixed8 x, Fixed8 y)
        {
            x.value = checked(x.value - y.value);
            return x;
        }
    }
}
