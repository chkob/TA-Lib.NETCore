using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode LinearRegIntercept(
            ref Span<decimal> inReal,
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

            if (inReal == null || outReal == null || optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = LinearRegInterceptLookback(optInTimePeriod);
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

            decimal sumX = optInTimePeriod * (optInTimePeriod - 1) * 0.5m;
            decimal sumXSqr = optInTimePeriod * (optInTimePeriod - 1) * (optInTimePeriod * 2 - 1) / 6.0m;
            decimal divisor = sumX * sumX - optInTimePeriod * sumXSqr;
            while (today <= endIdx)
            {
                decimal sumXY = default;
                decimal sumY = default;
                for (int i = optInTimePeriod; i-- != 0;)
                {
                    decimal tempValue1 = inReal[today - i];
                    sumY += tempValue1;
                    sumXY += i * tempValue1;
                }

                decimal m = (optInTimePeriod * sumXY - sumX * sumY) / divisor;
                outReal[outIdx++] = (sumY - m * sumX) / optInTimePeriod;
                today++;
            }

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int LinearRegInterceptLookback(int optInTimePeriod = 14)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod - 1;
        }
    }
}
