using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Mom(
            ref Span<double> inReal,
            int startIdx,
            int endIdx,
            ref Span<double> outReal,
            out int outBegIdx,
            out int outNbElement,
            int optInTimePeriod = 10)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inReal == null || outReal == null || optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = MomLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            int outIdx = default;
            int inIdx = startIdx;
            int trailingIdx = startIdx - lookbackTotal;
            while (inIdx <= endIdx)
            {
                outReal[outIdx++] = inReal[inIdx++] - inReal[trailingIdx++];
            }

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int MomLookback(int optInTimePeriod = 10)
        {
            if (optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod;
        }
    }
}
