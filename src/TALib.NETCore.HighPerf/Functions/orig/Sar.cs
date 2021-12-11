using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Sar(
            ref Span<double> inHigh,
            ref Span<double> inLow,
            int startIdx,
            int endIdx,
            ref Span<double> outReal,
            out int outBegIdx,
            out int outNbElement,
            double optInAcceleration = 0.02,
            double optInMaximum = 0.2)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inHigh == null || inLow == null || outReal == null || optInAcceleration < 0.0 || optInMaximum < 0.0)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = SarLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            double af = optInAcceleration;
            if (af > optInMaximum)
            {
                af = optInAcceleration = optInMaximum;
            }

            var epTemp = BufferHelpers.New(1);
            RetCode retCode = MinusDM(ref inHigh, ref inLow, startIdx, startIdx, ref epTemp, out _, out _, 1);
            if (retCode != RetCode.Success)
            {
                return retCode;
            }

            outBegIdx = startIdx;
            double sar;
            double ep;
            int outIdx = default;

            int todayIdx = startIdx;

            double newHigh = inHigh[todayIdx - 1];
            double newLow = inLow[todayIdx - 1];
            var isLong = epTemp[0] <= 0.0;
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

            newLow = inLow[todayIdx];
            newHigh = inHigh[todayIdx];

            while (todayIdx <= endIdx)
            {
                double prevLow = newLow;
                double prevHigh = newHigh;
                newLow = inLow[todayIdx];
                newHigh = inHigh[todayIdx];
                todayIdx++;

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

                        outReal[outIdx++] = sar;

                        af = optInAcceleration;
                        ep = newLow;

                        sar += af * (ep - sar);
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
                            af += optInAcceleration;
                            if (af > optInMaximum)
                            {
                                af = optInMaximum;
                            }
                        }

                        sar += af * (ep - sar);
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

                    outReal[outIdx++] = sar;

                    af = optInAcceleration;
                    ep = newHigh;

                    sar += af * (ep - sar);
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
                    outReal[outIdx++] = sar;

                    if (newLow < ep)
                    {
                        ep = newLow;
                        af += optInAcceleration;
                        if (af > optInMaximum)
                        {
                            af = optInMaximum;
                        }
                    }

                    sar += af * (ep - sar);

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

        public static int SarLookback() => 1;
    }
}
