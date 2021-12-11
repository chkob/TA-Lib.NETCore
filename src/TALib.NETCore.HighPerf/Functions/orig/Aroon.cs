using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Aroon(
            ref Span<double> inHigh,
            ref Span<double> inLow,
            int startIdx,
            int endIdx,
            ref Span<double> outAroonDown,
            ref Span<double> outAroonUp,
            out int outBegIdx,
            out int outNbElement,
            int optInTimePeriod = 14)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inHigh == null || inLow == null || outAroonDown == null || outAroonUp == null || optInTimePeriod < 2 ||
                optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = AroonLookback(optInTimePeriod);
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
            int lowestIdx = -1;
            int highestIdx = -1;
            double lowest = default;
            double highest = default;
            double factor = 100.0 / optInTimePeriod;

            while (today <= endIdx)
            {
                double tmp = inLow[today];
                if (lowestIdx < trailingIdx)
                {
                    lowestIdx = trailingIdx;
                    lowest = inLow[lowestIdx];
                    int i = lowestIdx;
                    while (++i <= today)
                    {
                        tmp = inLow[i];
                        if (tmp <= lowest)
                        {
                            lowestIdx = i;
                            lowest = tmp;
                        }
                    }
                }
                else if (tmp <= lowest)
                {
                    lowestIdx = today;
                    lowest = tmp;
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
                        if (tmp >= highest)
                        {
                            highestIdx = i;
                            highest = tmp;
                        }
                    }
                }
                else if (tmp >= highest)
                {
                    highestIdx = today;
                    highest = tmp;
                }

                outAroonUp[outIdx] = factor * (optInTimePeriod - (today - highestIdx));
                outAroonDown[outIdx] = factor * (optInTimePeriod - (today - lowestIdx));

                outIdx++;
                trailingIdx++;
                today++;
            }

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int AroonLookback(int optInTimePeriod = 14)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod;
        }
    }
}
