using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode ThreeOutside(
            ref Span<double> inOpen,
            ref Span<double> inHigh,
            ref Span<double> inLow,
            ref Span<double> inClose,
            int startIdx,
            int endIdx,
            int[] outInteger,
            out int outBegIdx,
            out int outNbElement)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inOpen == null || inHigh == null || inLow == null || inClose == null || outInteger == null)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = ThreeOutsideLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            int i = startIdx;
            int outIdx = default;
            do
            {
                if (CandleColor(ref inClose, ref inOpen, i - 1) && !CandleColor(ref inClose, ref inOpen, i - 2) &&
                    inClose[i - 1] > inOpen[i - 2] && inOpen[i - 1] < inClose[i - 2] &&
                    inClose[i] > inClose[i - 1]
                    ||
                    !CandleColor(ref inClose, ref inOpen, i - 1) && CandleColor(ref inClose, ref inOpen, i - 2) &&
                    inOpen[i - 1] > inClose[i - 2] && inClose[i - 1] < inOpen[i - 2] &&
                    inClose[i] < inClose[i - 1])
                {
                    outInteger[outIdx++] = Convert.ToInt32(CandleColor(ref inClose, ref inOpen, i - 1)) * 100;
                }
                else
                {
                    outInteger[outIdx++] = 0;
                }

                i++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }
        
        public static int ThreeOutsideLookback() => 3;
    }
}
