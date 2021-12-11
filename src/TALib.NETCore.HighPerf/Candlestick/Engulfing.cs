using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Engulfing(
            ref Span<decimal> inOpen,
            ref Span<decimal> inHigh,
            ref Span<decimal> inLow,
            ref Span<decimal> inClose,
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

            int lookbackTotal = EngulfingLookback();
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
                if (CandleColor(ref inClose, ref inOpen, i) && !CandleColor(ref inClose, ref inOpen, i - 1) &&            // white engulfs black
                    (inClose[i] >= inOpen[i - 1] && inOpen[i] < inClose[i - 1] ||
                     inClose[i] > inOpen[i - 1] && inOpen[i] <= inClose[i - 1]
                    )
                    ||
                    !CandleColor(ref inClose, ref inOpen, i) && CandleColor(ref inClose, ref inOpen, i - 1) &&            // black engulfs white
                    (inOpen[i] >= inClose[i - 1] && inClose[i] < inOpen[i - 1] ||
                     inOpen[i] > inClose[i - 1] && inClose[i] <= inOpen[i - 1]
                    )
                )
                {
                    if (!inOpen[i].Equals(inClose[i - 1]) && !inClose[i].Equals(inOpen[i - 1]))
                    {
                        outInteger[outIdx++] = Convert.ToInt32(CandleColor(ref inClose, ref inOpen, i)) * 100;
                    }
                    else
                    {
                        outInteger[outIdx++] = Convert.ToInt32(CandleColor(ref inClose, ref inOpen, i)) * 80;
                    }
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

        public static int EngulfingLookback() => 2;
    }
}
