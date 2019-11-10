﻿using System;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents a Q31.32 fixed-point number.
/// </summary>
public struct Fix64 : IEquatable<Fix64>, IComparable<Fix64>
{
    // Precision of this type is 2^-32, that is 2,3283064365386962890625E-10
    public static readonly decimal Precision = (decimal)(new Fix64(1L));//0.00000000023283064365386962890625m;
    public static readonly Fix64 MaxValue = new Fix64(MAX_VALUE);
    public static readonly Fix64 MinValue = new Fix64(MIN_VALUE);
    public static readonly Fix64 One = new Fix64(ONE);
    public static readonly Fix64 Zero = new Fix64();
    /// <summary>
    /// The value of Pi
    /// </summary>
    public static readonly Fix64 Pi = new Fix64(PI);
    public static readonly Fix64 PiOver2 = new Fix64(PI_OVER_2);
    public static readonly Fix64 PiTimes2 = new Fix64(PI_TIMES_2);
    public static readonly Fix64 PiInv = (Fix64)0.3183098861837906715377675267M;
    public static readonly Fix64 PiOver2Inv = (Fix64)0.6366197723675813430755350535M;

    const long MAX_VALUE = long.MaxValue;
    const long MIN_VALUE = long.MinValue;
    const int NUM_BITS = 64;
    const int FRACTIONAL_PLACES = 32;
    const long ONE = 1L << FRACTIONAL_PLACES;
    const long PI_TIMES_2 = 0x6487ED511;
    const long PI = 0x3243F6A88;
    const long PI_OVER_2 = 0x1921FB544;

    /// <summary>
    /// Returns a number indicating the sign of a Fix64 number.
    /// Returns 1 if the value is positive, 0 if is 0, and -1 if it is negative.
    /// </summary>
    public static int Sign(Fix64 value)
    {
        return
            value.RawValue < 0 ? -1 :
            value.RawValue > 0 ? 1 :
            0;
    }


    /// <summary>
    /// Returns the absolute value of a Fix64 number.
    /// Note: Abs(Fix64.MinValue) == Fix64.MaxValue.
    /// </summary>
    public static Fix64 Abs(Fix64 value)
    {
        if (value.RawValue == MIN_VALUE)
        {
            return MaxValue;
        }

        // branchless implementation, see http://www.strchr.com/optimized_abs_function
        var mask = value.RawValue >> 63;
        return new Fix64((value.RawValue + mask) ^ mask);
    }

    /// <summary>
    /// Returns the absolute value of a Fix64 number.
    /// FastAbs(Fix64.MinValue) is undefined.
    /// </summary>
    public static Fix64 FastAbs(Fix64 value)
    {
        // branchless implementation, see http://www.strchr.com/optimized_abs_function
        var mask = value.RawValue >> 63;
        return new Fix64((value.RawValue + mask) ^ mask);
    }


    /// <summary>
    /// Returns the largest integer less than or equal to the specified number.
    /// </summary>
    public static Fix64 Floor(Fix64 value)
    {
        // Just zero out the fractional part
        return new Fix64((long)((ulong)value.RawValue & 0xFFFFFFFF00000000));
    }

    /// <summary>
    /// Returns the smallest integral value that is greater than or equal to the specified number.
    /// </summary>
    public static Fix64 Ceiling(Fix64 value)
    {
        var hasFractionalPart = (value.RawValue & 0x00000000FFFFFFFF) != 0;
        return hasFractionalPart ? Floor(value) + One : value;
    }

    /// <summary>
    /// Rounds a value to the nearest integral value.
    /// If the value is halfway between an even and an uneven value, returns the even value.
    /// </summary>
    public static Fix64 Round(Fix64 value)
    {
        var fractionalPart = value.RawValue & 0x00000000FFFFFFFF;
        var integralPart = Floor(value);
        if (fractionalPart < 0x80000000)
        {
            return integralPart;
        }
        if (fractionalPart > 0x80000000)
        {
            return integralPart + One;
        }
        // if number is halfway between two values, round to the nearest even number
        // this is the method used by System.Math.Round().
        return (integralPart.RawValue & ONE) == 0
                    ? integralPart
                    : integralPart + One;
    }

    /// <summary>
    /// Adds x and y. Performs saturating addition, i.e. in case of overflow, 
    /// rounds to MinValue or MaxValue depending on sign of operands.
    /// </summary>
    public static Fix64 operator +(Fix64 x, Fix64 y)
    {
        var xl = x.RawValue;
        var yl = y.RawValue;
        var sum = xl + yl;
        // if signs of operands are equal and signs of sum and x are different
        if (((~(xl ^ yl) & (xl ^ sum)) & MIN_VALUE) != 0)
        {
            sum = xl > 0 ? MAX_VALUE : MIN_VALUE;
        }
        return new Fix64(sum);
    }

    /// <summary>
    /// Adds x and y witout performing overflow checking. Should be inlined by the CLR.
    /// </summary>
    public static Fix64 FastAdd(Fix64 x, Fix64 y)
    {
        return new Fix64(x.RawValue + y.RawValue);
    }

    /// <summary>
    /// Subtracts y from x. Performs saturating substraction, i.e. in case of overflow, 
    /// rounds to MinValue or MaxValue depending on sign of operands.
    /// </summary>
    public static Fix64 operator -(Fix64 x, Fix64 y)
    {
        var xl = x.RawValue;
        var yl = y.RawValue;
        var diff = xl - yl;
        // if signs of operands are different and signs of sum and x are different
        if ((((xl ^ yl) & (xl ^ diff)) & MIN_VALUE) != 0)
        {
            diff = xl < 0 ? MIN_VALUE : MAX_VALUE;
        }
        return new Fix64(diff);
    }

    /// <summary>
    /// Subtracts y from x witout performing overflow checking. Should be inlined by the CLR.
    /// </summary>
    public static Fix64 FastSub(Fix64 x, Fix64 y)
    {
        return new Fix64(x.RawValue - y.RawValue);
    }

    static long AddOverflowHelper(long x, long y, ref bool overflow)
    {
        var sum = x + y;
        // x + y overflows if sign(x) ^ sign(y) != sign(sum)
        overflow |= ((x ^ y ^ sum) & MIN_VALUE) != 0;
        return sum;
    }

    public static Fix64 operator *(Fix64 x, Fix64 y)
    {

        var xl = x.RawValue;
        var yl = y.RawValue;

        var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
        var xhi = xl >> FRACTIONAL_PLACES;
        var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
        var yhi = yl >> FRACTIONAL_PLACES;

        var lolo = xlo * ylo;
        var lohi = (long)xlo * yhi;
        var hilo = xhi * (long)ylo;
        var hihi = xhi * yhi;

        var loResult = lolo >> FRACTIONAL_PLACES;
        var midResult1 = lohi;
        var midResult2 = hilo;
        var hiResult = hihi << FRACTIONAL_PLACES;

        bool overflow = false;
        var sum = AddOverflowHelper((long)loResult, midResult1, ref overflow);
        sum = AddOverflowHelper(sum, midResult2, ref overflow);
        sum = AddOverflowHelper(sum, hiResult, ref overflow);

        bool opSignsEqual = ((xl ^ yl) & MIN_VALUE) == 0;

        // if signs of operands are equal and sign of result is negative,
        // then multiplication overflowed positively
        // the reverse is also true
        if (opSignsEqual)
        {
            if (sum < 0 || (overflow && xl > 0))
            {
                return MaxValue;
            }
        }
        else
        {
            if (sum > 0)
            {
                return MinValue;
            }
        }

        // if the top 32 bits of hihi (unused in the result) are neither all 0s or 1s,
        // then this means the result overflowed.
        var topCarry = hihi >> FRACTIONAL_PLACES;
        if (topCarry != 0 && topCarry != -1 /*&& xl != -17 && yl != -17*/)
        {
            return opSignsEqual ? MaxValue : MinValue;
        }

        // If signs differ, both operands' magnitudes are greater than 1,
        // and the result is greater than the negative operand, then there was negative overflow.
        if (!opSignsEqual)
        {
            long posOp, negOp;
            if (xl > yl)
            {
                posOp = xl;
                negOp = yl;
            }
            else
            {
                posOp = yl;
                negOp = xl;
            }
            if (sum > negOp && negOp < -ONE && posOp > ONE)
            {
                return MinValue;
            }
        }

        return new Fix64(sum);
    }

    /// <summary>
    /// Performs multiplication without checking for overflow.
    /// Useful for performance-critical code where the values are guaranteed not to cause overflow
    /// </summary>
    public static Fix64 FastMul(Fix64 x, Fix64 y)
    {

        var xl = x.RawValue;
        var yl = y.RawValue;

        var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
        var xhi = xl >> FRACTIONAL_PLACES;
        var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
        var yhi = yl >> FRACTIONAL_PLACES;

        var lolo = xlo * ylo;
        var lohi = (long)xlo * yhi;
        var hilo = xhi * (long)ylo;
        var hihi = xhi * yhi;

        var loResult = lolo >> FRACTIONAL_PLACES;
        var midResult1 = lohi;
        var midResult2 = hilo;
        var hiResult = hihi << FRACTIONAL_PLACES;

        var sum = (long)loResult + midResult1 + midResult2 + hiResult;
        return new Fix64(sum);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int CountLeadingZeroes(ulong x)
    {
        int result = 0;
        while ((x & 0xF000000000000000) == 0) { result += 4; x <<= 4; }
        while ((x & 0x8000000000000000) == 0) { result += 1; x <<= 1; }
        return result;
    }

    public static Fix64 operator /(Fix64 x, Fix64 y)
    {
        var xl = x.RawValue;
        var yl = y.RawValue;

        if (yl == 0)
        {
            throw new DivideByZeroException();
        }

        var remainder = (ulong)(xl >= 0 ? xl : -xl);
        var divider = (ulong)(yl >= 0 ? yl : -yl);
        var quotient = 0UL;
        var bitPos = NUM_BITS / 2 + 1;


        // If the divider is divisible by 2^n, take advantage of it.
        while ((divider & 0xF) == 0 && bitPos >= 4)
        {
            divider >>= 4;
            bitPos -= 4;
        }

        while (remainder != 0 && bitPos >= 0)
        {
            int shift = CountLeadingZeroes(remainder);
            if (shift > bitPos)
            {
                shift = bitPos;
            }
            remainder <<= shift;
            bitPos -= shift;

            var div = remainder / divider;
            remainder = remainder % divider;
            quotient += div << bitPos;

            // Detect overflow
            if ((div & ~(0xFFFFFFFFFFFFFFFF >> bitPos)) != 0)
            {
                return ((xl ^ yl) & MIN_VALUE) == 0 ? MaxValue : MinValue;
            }

            remainder <<= 1;
            --bitPos;
        }

        // rounding
        ++quotient;
        var result = (long)(quotient >> 1);
        if (((xl ^ yl) & MIN_VALUE) != 0)
        {
            result = -result;
        }

        return new Fix64(result);
    }

    public static Fix64 operator %(Fix64 x, Fix64 y)
    {
        return new Fix64(
            x.RawValue == MIN_VALUE & y.RawValue == -1 ?
            0 :
            x.RawValue % y.RawValue);
    }

    /// <summary>
    /// Performs modulo as fast as possible; throws if x == MinValue and y == -1.
    /// Use the operator (%) for a more reliable but slower modulo.
    /// </summary>
    public static Fix64 FastMod(Fix64 x, Fix64 y)
    {
        return new Fix64(x.RawValue % y.RawValue);
    }

    public static Fix64 operator -(Fix64 x)
    {
        return x.RawValue == MIN_VALUE ? MaxValue : new Fix64(-x.RawValue);
    }

    public static bool operator ==(Fix64 x, Fix64 y)
    {
        return x.RawValue == y.RawValue;
    }

    public static bool operator !=(Fix64 x, Fix64 y)
    {
        return x.RawValue != y.RawValue;
    }

    public static bool operator >(Fix64 x, Fix64 y)
    {
        return x.RawValue > y.RawValue;
    }

    public static bool operator <(Fix64 x, Fix64 y)
    {
        return x.RawValue < y.RawValue;
    }

    public static bool operator >=(Fix64 x, Fix64 y)
    {
        return x.RawValue >= y.RawValue;
    }

    public static bool operator <=(Fix64 x, Fix64 y)
    {
        return x.RawValue <= y.RawValue;
    }



    public static explicit operator Fix64(long value)
    {
        return new Fix64(value * ONE);
    }
    public static explicit operator long(Fix64 value)
    {
        return value.RawValue >> FRACTIONAL_PLACES;
    }
    public static explicit operator Fix64(float value)
    {
        return new Fix64((long)(value * ONE));
    }
    public static explicit operator float(Fix64 value)
    {
        return (float)value.RawValue / ONE;
    }
    public static explicit operator Fix64(double value)
    {
        return new Fix64((long)(value * ONE));
    }
    public static explicit operator double(Fix64 value)
    {
        return (double)value.RawValue / ONE;
    }
    public static explicit operator Fix64(decimal value)
    {
        return new Fix64((long)(value * ONE));
    }
    public static explicit operator decimal(Fix64 value)
    {
        return (decimal)value.RawValue / ONE;
    }

    public override bool Equals(object obj)
    {
        return obj is Fix64 && ((Fix64)obj).RawValue == RawValue;
    }

    public override int GetHashCode()
    {
        return RawValue.GetHashCode();
    }

    public bool Equals(Fix64 other)
    {
        return RawValue == other.RawValue;
    }

    public int CompareTo(Fix64 other)
    {
        return RawValue.CompareTo(other.RawValue);
    }

    public override string ToString()
    {
        // Up to 10 decimal places
        return ((decimal)this).ToString("0.##########");
    }

    public static Fix64 FromRaw(long rawValue)
    {
        return new Fix64(rawValue);
    }



    /// <summary>
    /// The underlying integer representation
    /// </summary>
    public long RawValue { get; }

    /// <summary>
    /// This is the constructor from raw value; it can only be used interally.
    /// </summary>
    /// <param name="rawValue"></param>
    Fix64(long rawValue)
    {
        RawValue = rawValue;
    }

    public Fix64(int value)
    {
        RawValue = value * ONE;
    }
}

public struct FixVector2D
{
    public Fix64 X;
    public Fix64 Y;

    public FixVector2D(Fix64 x, Fix64 y)
    {
        X = x;
        Y = y;
    }

    #region Vector Operations
    public static FixVector2D VectorAdd(FixVector2D F1, FixVector2D F2)
    {
        FixVector2D result;
        result.X = F1.X + F2.X;
        result.Y = F1.Y + F2.Y;
        return result;
    }

    public static FixVector2D VectorSubtract(FixVector2D F1, FixVector2D F2)
    {
        FixVector2D result;
        result.X = F1.X - F2.X;
        result.Y = F1.Y - F2.Y;
        return result;
    }

    public static FixVector2D VectorDivide(FixVector2D F1, int Divisor)
    {
        FixVector2D result;
        result.X = F1.X / (Fix64)Divisor;
        result.Y = F1.Y / (Fix64)Divisor;
        return result;
    }
    #endregion
}