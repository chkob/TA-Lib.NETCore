using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode XSideGap3Methods(
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

            int lookbackTotal = XSideGap3MethodsLookback();
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
                if (CandleColor(ref inClose, ref inOpen, i - 2) == CandleColor(ref inClose, ref inOpen, i - 1) && // 1st and 2nd of same color
                    CandleColor(ref inClose, ref inOpen, i - 1) == !CandleColor(ref inClose, ref inOpen, i) && // 3rd opposite color
                    inOpen[i] < Math.Max(inClose[i - 1], inOpen[i - 1]) && // 3rd opens within 2nd rb
                    inOpen[i] > Math.Min(inClose[i - 1], inOpen[i - 1]) &&
                    inClose[i] < Math.Max(inClose[i - 2], inOpen[i - 2]) && // 3rd closes within 1st rb
                    inClose[i] > Math.Min(inClose[i - 2], inOpen[i - 2]) &&
                    (CandleColor(ref inClose, ref inOpen, i - 2) && // when 1st is white
                     RealBodyGapUp(ref inOpen, ref inClose, i - 1, i - 2) // upside gap
                     ||
                     !CandleColor(ref inClose, ref inOpen, i - 2) && // when 1st is black
                     RealBodyGapDown(ref inOpen, ref inClose, i - 1, i - 2))) // downside gap
                {
                    outInteger[outIdx++] = Convert.ToInt32(CandleColor(ref inClose, ref inOpen, i - 2)) * 100;
                }
                else
                {
                    outInteger[outIdx++] = 0;
                }

                /* add the current range and subtract the first range: this is done after the pattern recognition
                 * when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                 */
                i++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int XSideGap3MethodsLookback() => 2;
    }
}
