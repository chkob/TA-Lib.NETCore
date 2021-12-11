using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Correl(
            ref Span<decimal> inReal0,
            ref Span<decimal> inReal1,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
            out int outBegIdx,
            out int outNbElement,
            int optInTimePeriod = 30)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inReal0 == null || inReal1 == null || outReal == null || optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = CorrelLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            outBegIdx = startIdx;
            int trailingIdx = startIdx - lookbackTotal;

            decimal sumX, sumY, sumX2, sumY2;
            decimal sumXY = sumX = sumY = sumX2 = sumY2 = default;
            int today;
            for (today = trailingIdx; today <= startIdx; today++)
            {
                decimal x = inReal0[today];
                sumX += x;
                sumX2 += x * x;

                decimal y = inReal1[today];
                sumXY += x * y;
                sumY += y;
                sumY2 += y * y;
            }

            decimal trailingX = inReal0[trailingIdx];
            decimal trailingY = inReal1[trailingIdx++];
            decimal tempReal = (sumX2 - sumX * sumX / optInTimePeriod) * (sumY2 - sumY * sumY / optInTimePeriod);
            outReal[0] = !HighPerf.Lib.IsZeroOrNeg(tempReal) ? (sumXY - sumX * sumY / optInTimePeriod) / (decimal)Math.Sqrt((double)tempReal) : 0.0m;

            int outIdx = 1;
            while (today <= endIdx)
            {
                sumX -= trailingX;
                sumX2 -= trailingX * trailingX;

                sumXY -= trailingX * trailingY;
                sumY -= trailingY;
                sumY2 -= trailingY * trailingY;

                decimal x = inReal0[today];
                sumX += x;
                sumX2 += x * x;

                decimal y = inReal1[today++];
                sumXY += x * y;
                sumY += y;
                sumY2 += y * y;

                trailingX = inReal0[trailingIdx];
                trailingY = inReal1[trailingIdx++];
                tempReal = (sumX2 - sumX * sumX / optInTimePeriod) * (sumY2 - sumY * sumY / optInTimePeriod);
                outReal[outIdx++] = !HighPerf.Lib.IsZeroOrNeg(tempReal) ? (sumXY - sumX * sumY / optInTimePeriod) / (decimal)Math.Sqrt((double)tempReal) : 0.0m;
            }

            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int CorrelLookback(int optInTimePeriod = 30)
        {
            if (optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod - 1;
        }
    }
}
