using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode MidPrice(
            ref Span<decimal> inHigh,
            ref Span<decimal> inLow,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
            out int outBegIdx,
            out int outNbElement,
            int optInTimePeriod = 14)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inHigh == null || inLow == null || outReal == null || optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = MidPriceLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            int outIdx = default;
            int today = startIdx;
            int trailingIdx = startIdx - lookbackTotal;
            while (today <= endIdx)
            {
                decimal lowest = inLow[trailingIdx];
                decimal highest = inHigh[trailingIdx++];
                for (int i = trailingIdx; i <= today; i++)
                {
                    decimal tmp = inLow[i];
                    if (tmp < lowest)
                    {
                        lowest = tmp;
                    }

                    tmp = inHigh[i];
                    if (tmp > highest)
                    {
                        highest = tmp;
                    }
                }

                outReal[outIdx++] = (highest + lowest) / 2.0m;
                today++;
            }

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int MidPriceLookback(int optInTimePeriod = 14)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod - 1;
        }
    }
}
