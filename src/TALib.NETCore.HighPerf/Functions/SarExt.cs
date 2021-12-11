using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode SarExt(
            ref Span<decimal> inHigh,
            ref Span<decimal> inLow,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
            out int outBegIdx,
            out int outNbElement,
            decimal optInStartValue = 0.0m,
            decimal optInOffsetOnReverse = 0.0m,
            decimal optInAccelerationInitLong = 0.02m,
            decimal optInAccelerationLong = 0.02m,
            decimal optInAccelerationMaxLong = 0.2m,
            decimal optInAccelerationInitShort = 0.02m,
            decimal optInAccelerationShort = 0.02m,
            decimal optInAccelerationMaxShort = 0.2m)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inHigh == null || inLow == null || outReal == null || optInOffsetOnReverse < 0.0m || optInAccelerationInitLong < 0.0m ||
                optInAccelerationLong < 0.0m || optInAccelerationMaxLong < 0.0m || optInAccelerationInitShort < 0.0m ||
                optInAccelerationShort < 0.0m || optInAccelerationMaxShort < 0.0m)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = SarExtLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            decimal sar;
            decimal ep;
            bool isLong;

            decimal afLong = optInAccelerationInitLong;
            decimal afShort = optInAccelerationInitShort;
            if (afLong > optInAccelerationMaxLong)
            {
                optInAccelerationInitLong = optInAccelerationMaxLong;
                afLong = optInAccelerationInitLong;
            }

            if (optInAccelerationLong > optInAccelerationMaxLong)
            {
                optInAccelerationLong = optInAccelerationMaxLong;
            }

            if (afShort > optInAccelerationMaxShort)
            {
                optInAccelerationInitShort = optInAccelerationMaxShort;
                afShort = optInAccelerationInitShort;
            }

            if (optInAccelerationShort > optInAccelerationMaxShort)
            {
                optInAccelerationShort = optInAccelerationMaxShort;
            }

            if (optInStartValue.Equals(0.0))
            {
                var epTemp = BufferHelpers.New(1);
                RetCode retCode = MinusDM(ref inHigh, ref inLow, startIdx, startIdx, ref epTemp, out _, out _, 1);
                if (retCode != RetCode.Success)
                {
                    return retCode;
                }

                isLong = epTemp[0] <= 0.0m;
            }
            else if (optInStartValue > 0.0m)
            {
                isLong = true;
            }
            else
            {
                isLong = false;
            }

            outBegIdx = startIdx;
            int outIdx = default;

            int todayIdx = startIdx;

            decimal newHigh = inHigh[todayIdx - 1];
            decimal newLow = inLow[todayIdx - 1];
            if (optInStartValue.Equals(0.0))
            {
                if (isLong)
                {
                    ep = inHigh[todayIdx];
                    sar = newLow;
                }
                else
                {
                    ep = inLow[todayIdx];
                    sar = newHigh;
                }
            }
            else if (optInStartValue > 0.0m)
            {
                ep = inHigh[todayIdx];
                sar = optInStartValue;
            }
            else
            {
                ep = inLow[todayIdx];
                sar = Math.Abs(optInStartValue);
            }

            newLow = inLow[todayIdx];
            newHigh = inHigh[todayIdx];

            while (todayIdx <= endIdx)
            {
                decimal prevLow = newLow;
                decimal prevHigh = newHigh;
                newLow = inLow[todayIdx];
                newHigh = inHigh[todayIdx++];
                if (isLong)
                {
                    if (newLow <= sar)
                    {
                        isLong = false;
                        sar = ep;

                        if (sar < prevHigh)
                        {
                            sar = prevHigh;
                        }

                        if (sar < newHigh)
                        {
                            sar = newHigh;
                        }

                        if (!optInOffsetOnReverse.Equals(0.0))
                        {
                            sar += sar * optInOffsetOnReverse;
                        }

                        outReal[outIdx++] = -sar;

                        afShort = optInAccelerationInitShort;
                        ep = newLow;

                        sar += afShort * (ep - sar);

                        if (sar < prevHigh)
                        {
                            sar = prevHigh;
                        }

                        if (sar < newHigh)
                        {
                            sar = newHigh;
                        }
                    }
                    else
                    {
                        outReal[outIdx++] = sar;
                        if (newHigh > ep)
                        {
                            ep = newHigh;
                            afLong += optInAccelerationLong;
                            if (afLong > optInAccelerationMaxLong)
                            {
                                afLong = optInAccelerationMaxLong;
                            }
                        }

                        sar += afLong * (ep - sar);

                        if (sar > prevLow)
                        {
                            sar = prevLow;
                        }

                        if (sar > newLow)
                        {
                            sar = newLow;
                        }
                    }
                }
                else if (newHigh >= sar)
                {
                    isLong = true;
                    sar = ep;

                    if (sar > prevLow)
                    {
                        sar = prevLow;
                    }

                    if (sar > newLow)
                    {
                        sar = newLow;
                    }

                    if (!optInOffsetOnReverse.Equals(0.0))
                    {
                        sar -= sar * optInOffsetOnReverse;
                    }

                    outReal[outIdx++] = sar;

                    afLong = optInAccelerationInitLong;
                    ep = newHigh;

                    sar += afLong * (ep - sar);

                    if (sar > prevLow)
                    {
                        sar = prevLow;
                    }

                    if (sar > newLow)
                    {
                        sar = newLow;
                    }
                }
                else
                {
                    outReal[outIdx++] = -sar;
                    if (newLow < ep)
                    {
                        ep = newLow;
                        afShort += optInAccelerationShort;
                        if (afShort > optInAccelerationMaxShort)
                        {
                            afShort = optInAccelerationMaxShort;
                        }
                    }

                    sar += afShort * (ep - sar);

                    if (sar < prevHigh)
                    {
                        sar = prevHigh;
                    }

                    if (sar < newHigh)
                    {
                        sar = newHigh;
                    }
                }
            }

            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int SarExtLookback() => 1;
    }
}
