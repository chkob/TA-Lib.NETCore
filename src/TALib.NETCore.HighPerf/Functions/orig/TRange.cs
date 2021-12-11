using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode TRange(
            ref Span<double> inHigh,
            ref Span<double> inLow,
            ref Span<double> inClose,
            int startIdx,
            int endIdx,
            ref Span<double> outReal,
            out int outBegIdx,
            out int outNbElement)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inHigh == null || inLow == null || inClose == null || outReal == null)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = TRangeLookback();
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
            while (today <= endIdx)
            {
                double tempLT = inLow[today];
                double tempHT = inHigh[today];
                double tempCY = inClose[today - 1];
                double greatest = tempHT - tempLT;

                double val2 = Math.Abs(tempCY - tempHT);
                if (val2 > greatest)
                {
                    greatest = val2;
                }

                double val3 = Math.Abs(tempCY - tempLT);
                if (val3 > greatest)
                {
                    greatest = val3;
                }

                outReal[outIdx++] = greatest;
                today++;
            }

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int TRangeLookback() => 1;
    }
}
