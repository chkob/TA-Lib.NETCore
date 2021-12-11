using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode SarExt(
            ref Span<double> inHigh,
            ref Span<double> inLow,
            int startIdx,
            int endIdx,
            ref Span<double> outReal,
            out int outBegIdx,
            out int outNbElement,
            double optInStartValue = 0.0,
            double optInOffsetOnReverse = 0.0,
            double optInAccelerationInitLong = 0.02,
            double optInAccelerationLong = 0.02,
            double optInAccelerationMaxLong = 0.2,
            double optInAccelerationInitShort = 0.02,
            double optInAccelerationShort = 0.02,
            double optInAccelerationMaxShort = 0.2)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inHigh == null || inLow == null || outReal == null || optInOffsetOnReverse < 0.0 || optInAccelerationInitLong < 0.0 ||
                optInAccelerationLong < 0.0 || optInAccelerationMaxLong < 0.0 || optInAccelerationInitShort < 0.0 ||
                optInAccelerationShort < 0.0 || optInAccelerationMaxShort < 0.0)
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

            double sar;
            double ep;
            bool isLong;

            double afLong = optInAccelerationInitLong;
            double afShort = optInAccelerationInitShort;
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

                isLong = epTemp[0] <= 0.0;
            }
            else if (optInStartValue > 0.0)
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

            double newHigh = inHigh[todayIdx - 1];
            double newLow = inLow[todayIdx - 1];
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
            else if (optInStartValue > 0.0)
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
                double prevLow = newLow;
                double prevHigh = newHigh;
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
