using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode WillR(
            ref Span<decimal> inHigh,
            ref Span<decimal> inLow,
            ref Span<decimal> inClose,
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

            if (inHigh == null || inLow == null || inClose == null || outReal == null || optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = WillRLookback(optInTimePeriod);
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
            int highestIdx = -1;
            int lowestIdx = highestIdx;
            decimal highest, lowest;
            decimal diff = highest = lowest = default;

            while (today <= endIdx)
            {
                decimal tmp = inLow[today];
                if (lowestIdx < trailingIdx)
                {
                    lowestIdx = trailingIdx;
                    lowest = inLow[lowestIdx];
                    int i = lowestIdx;
                    while (++i <= today)
                    {
                        tmp = inLow[i];
                        if (tmp < lowest)
                        {
                            lowestIdx = i;
                            lowest = tmp;
                        }
                    }

                    diff = (highest - lowest) / -100.0m;
                }
                else if (tmp <= lowest)
                {
                    lowestIdx = today;
                    lowest = tmp;
                    diff = (highest - lowest) / -100.0m;
                }

                tmp = inHigh[today];
                if (highestIdx < trailingIdx)
                {
                    highestIdx = trailingIdx;
                    highest = inHigh[highestIdx];
                    int i = highestIdx;
                    while (++i <= today)
                    {
                        tmp = inHigh[i];
                        if (tmp > highest)
                        {
                            highestIdx = i;
                            highest = tmp;
                        }
                    }

                    diff = (highest - lowest) / -100.0m;
                }
                else if (tmp >= highest)
                {
                    highestIdx = today;
                    highest = tmp;
                    diff = (highest - lowest) / -100.0m;
                }

                outReal[outIdx++] = !diff.Equals(0.0) ? (highest - inClose[today]) / diff : 0.0m;

                trailingIdx++;
                today++;
            }

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int WillRLookback(int optInTimePeriod = 14)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod - 1;
        }
    }
}
