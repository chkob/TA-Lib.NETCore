using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode T3(
            ref Span<decimal> inReal,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
            out int outBegIdx,
            out int outNbElement,
            int optInTimePeriod = 5,
            decimal optInVFactor = 0.7m)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inReal == null || outReal == null || optInTimePeriod < 2 || optInTimePeriod > 100000 || optInVFactor < 0.0m ||
                optInVFactor > 1.0m)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = T3Lookback(optInTimePeriod);
            if (startIdx <= lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            outBegIdx = startIdx;
            int today = startIdx - lookbackTotal;

            decimal k = 2.0m / (optInTimePeriod + 1.0m);
            decimal oneMinusK = 1.0m - k;

            decimal tempReal = inReal[today++];
            for (int i = optInTimePeriod - 1; i > 0; i--)
            {
                tempReal += inReal[today++];
            }
            decimal e1 = tempReal / optInTimePeriod;

            tempReal = e1;
            for (int i = optInTimePeriod - 1; i > 0; i--)
            {
                e1 = k * inReal[today++] + oneMinusK * e1;
                tempReal += e1;
            }
            decimal e2 = tempReal / optInTimePeriod;

            tempReal = e2;
            for (int i = optInTimePeriod - 1; i > 0; i--)
            {
                e1 = k * inReal[today++] + oneMinusK * e1;
                e2 = k * e1 + oneMinusK * e2;
                tempReal += e2;
            }
            decimal e3 = tempReal / optInTimePeriod;

            tempReal = e3;
            for (int i = optInTimePeriod - 1; i > 0; i--)
            {
                e1 = k * inReal[today++] + oneMinusK * e1;
                e2 = k * e1 + oneMinusK * e2;
                e3 = k * e2 + oneMinusK * e3;
                tempReal += e3;
            }
            decimal e4 = tempReal / optInTimePeriod;

            tempReal = e4;
            for (int i = optInTimePeriod - 1; i > 0; i--)
            {
                e1 = k * inReal[today++] + oneMinusK * e1;
                e2 = k * e1 + oneMinusK * e2;
                e3 = k * e2 + oneMinusK * e3;
                e4 = k * e3 + oneMinusK * e4;
                tempReal += e4;
            }
            decimal e5 = tempReal / optInTimePeriod;

            tempReal = e5;
            for (int i = optInTimePeriod - 1; i > 0; i--)
            {
                e1 = k * inReal[today++] + oneMinusK * e1;
                e2 = k * e1 + oneMinusK * e2;
                e3 = k * e2 + oneMinusK * e3;
                e4 = k * e3 + oneMinusK * e4;
                e5 = k * e4 + oneMinusK * e5;
                tempReal += e5;
            }
            decimal e6 = tempReal / optInTimePeriod;

            while (today <= startIdx)
            {
                e1 = k * inReal[today++] + oneMinusK * e1;
                e2 = k * e1 + oneMinusK * e2;
                e3 = k * e2 + oneMinusK * e3;
                e4 = k * e3 + oneMinusK * e4;
                e5 = k * e4 + oneMinusK * e5;
                e6 = k * e5 + oneMinusK * e6;
            }

            tempReal = optInVFactor * optInVFactor;
            decimal c1 = -(tempReal * optInVFactor);
            decimal c2 = 3.0m * (tempReal - c1);
            decimal c3 = -6.0m * tempReal - 3.0m * (optInVFactor - c1);
            decimal c4 = 1.0m + 3.0m * optInVFactor - c1 + 3.0m * tempReal;

            int outIdx = default;
            outReal[outIdx++] = c1 * e6 + c2 * e5 + c3 * e4 + c4 * e3;

            while (today <= endIdx)
            {
                e1 = k * inReal[today++] + oneMinusK * e1;
                e2 = k * e1 + oneMinusK * e2;
                e3 = k * e2 + oneMinusK * e3;
                e4 = k * e3 + oneMinusK * e4;
                e5 = k * e4 + oneMinusK * e5;
                e6 = k * e5 + oneMinusK * e6;
                outReal[outIdx++] = c1 * e6 + c2 * e5 + c3 * e4 + c4 * e3;
            }

            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int T3Lookback(int optInTimePeriod = 5)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return (optInTimePeriod - 1) * 6 + (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.T3];
        }
    }
}
