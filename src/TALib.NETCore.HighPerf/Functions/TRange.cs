using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode TRange(
            ref Span<decimal> inHigh,
            ref Span<decimal> inLow,
            ref Span<decimal> inClose,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
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
                decimal tempLT = inLow[today];
                decimal tempHT = inHigh[today];
                decimal tempCY = inClose[today - 1];
                decimal greatest = tempHT - tempLT;

                decimal val2 = Math.Abs(tempCY - tempHT);
                if (val2 > greatest)
                {
                    greatest = val2;
                }

                decimal val3 = Math.Abs(tempCY - tempLT);
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
