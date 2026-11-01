using System;
using System.Globalization;

namespace DoubleDown.Scoring
{
    /// <summary>Non-negative decimal magnitude for scores beyond Int64.</summary>
    public readonly struct BigNumber : IComparable<BigNumber>, IEquatable<BigNumber>
    {
        public double Mantissa { get; }
        public int Exponent { get; }
        public static BigNumber Zero => default;
        public bool IsZero => Mantissa == 0d;

        public BigNumber(double mantissa, int exponent = 0)
        {
            if (double.IsNaN(mantissa) || double.IsInfinity(mantissa) || mantissa < 0d)
                throw new ArgumentOutOfRangeException(nameof(mantissa));
            if (mantissa == 0d) { Mantissa = 0d; Exponent = 0; return; }
            var shift = (int)Math.Floor(Math.Log10(mantissa));
            var normalized = mantissa / Math.Pow(10d, shift);
            if (normalized >= 10d) { normalized /= 10d; shift++; }
            if (normalized < 1d) { normalized *= 10d; shift--; }
            Mantissa = normalized;
            Exponent = checked(exponent + shift);
        }

        public static BigNumber From(long value) => new BigNumber(value);
        public int CompareTo(BigNumber other)
        {
            if (IsZero) return other.IsZero ? 0 : -1;
            if (other.IsZero) return 1;
            return Exponent != other.Exponent
                ? Exponent.CompareTo(other.Exponent) : Mantissa.CompareTo(other.Mantissa);
        }
        public bool Equals(BigNumber other) => Mantissa.Equals(other.Mantissa) && Exponent == other.Exponent;
        public override bool Equals(object obj) => obj is BigNumber other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Mantissa, Exponent);
        public static bool operator >=(BigNumber a, BigNumber b) => a.CompareTo(b) >= 0;
        public static bool operator <=(BigNumber a, BigNumber b) => a.CompareTo(b) <= 0;
        public static bool operator >(BigNumber a, BigNumber b) => a.CompareTo(b) > 0;
        public static bool operator <(BigNumber a, BigNumber b) => a.CompareTo(b) < 0;
        public static bool operator ==(BigNumber a, BigNumber b) => a.Equals(b);
        public static bool operator !=(BigNumber a, BigNumber b) => !a.Equals(b);
        public static BigNumber operator +(BigNumber a, BigNumber b)
        {
            if (a.IsZero) return b;
            if (b.IsZero) return a;
            if (a.Exponent < b.Exponent) (a, b) = (b, a);
            var gap = a.Exponent - (long)b.Exponent;
            return gap > 16 ? a : new BigNumber(a.Mantissa + b.Mantissa * Math.Pow(10d, -gap), a.Exponent);
        }
        public static BigNumber operator *(BigNumber a, BigNumber b) => a.IsZero || b.IsZero
            ? Zero : new BigNumber(a.Mantissa * b.Mantissa, checked(a.Exponent + b.Exponent));
        public static BigNumber operator *(BigNumber a, double b) => a.IsZero || b == 0d
            ? Zero : new BigNumber(a.Mantissa * b, a.Exponent);

        public static BigNumber Pow(double basis, int power)
        {
            if (basis < 0d || power < 0) throw new ArgumentOutOfRangeException();
            var result = new BigNumber(1d);
            var factor = new BigNumber(basis);
            while (power > 0)
            {
                if ((power & 1) != 0) result *= factor;
                power >>= 1;
                if (power > 0) factor *= factor;
            }
            return result;
        }

        public override string ToString()
        {
            if (IsZero) return "0";
            if (Exponent < 3)
            {
                var value = Mantissa * Math.Pow(10d, Exponent);
                return value.ToString(Math.Abs(value - Math.Round(value)) < 0.0000001d
                    ? "N0" : "0.#", CultureInfo.InvariantCulture);
            }
            var group = Exponent / 3;
            var suffixes = new[] { "", "K", "M", "B", "T", "Qa", "Qi" };
            if (group < suffixes.Length)
                return (Mantissa * Math.Pow(10d, Exponent % 3))
                    .ToString("0.#", CultureInfo.InvariantCulture) + suffixes[group];
            return Mantissa.ToString("0.##", CultureInfo.InvariantCulture) +
                "e" + Exponent.ToString(CultureInfo.InvariantCulture);
        }
    }
}
