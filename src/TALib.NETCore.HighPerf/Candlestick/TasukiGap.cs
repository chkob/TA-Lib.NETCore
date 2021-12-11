using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode TasukiGap(
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

            int lookbackTotal = TasukiGapLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            double nearPeriodTotal = default;
            int nearTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.Near);
            int i = nearTrailingIdx;
            while (i < startIdx)
            {
                nearPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, i - 1);
                i++;
            }

            i = startIdx;
            int outIdx = default;
            do
            {
                if (RealBodyGapUp(ref inOpen, ref inClose, i - 1, i - 2) && // upside gap
                    CandleColor(ref inClose, ref inOpen, i - 1) && // 1st: white
                    !CandleColor(ref inClose, ref inOpen, i) && // 2nd: black
                    inOpen[i] < inClose[i - 1] && inOpen[i] > inOpen[i - 1] && //      that opens within the white rb
                    inClose[i] < inOpen[i - 1] && //      and closes under the white rb
                    inClose[i] > Math.Max(inClose[i - 2], inOpen[i - 2]) && //      inside the gap
                    // size of 2 rb near the same
                    Math.Abs(RealBody(ref inClose, ref inOpen, i - 1) - RealBody(ref inClose, ref inOpen, i)) <
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, nearPeriodTotal, i - 1)
                    ||
                    RealBodyGapDown(ref inOpen, ref inClose, i - 1, i - 2) && // downside gap
                    !CandleColor(ref inClose, ref inOpen, i - 1) && // 1st: black
                    CandleColor(ref inClose, ref inOpen, i) && // 2nd: white
                    inOpen[i] < inOpen[i - 1] && inOpen[i] > inClose[i - 1] && //      that opens within the black rb
                    inClose[i] > inOpen[i - 1] && //      and closes above the black rb
                    inClose[i] < Math.Min(inClose[i - 2], inOpen[i - 2]) && //      inside the gap
                    // size of 2 rb near the same
                    Math.Abs(RealBody(ref inClose, ref inOpen, i - 1) - RealBody(ref inClose, ref inOpen, i)) <
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, nearPeriodTotal, i - 1))
                {
                    outInteger[outIdx++] = Convert.ToInt32(CandleColor(ref inClose, ref inOpen, i - 1)) * 100;
                }
                else
                {
                    outInteger[outIdx++] = 0;
                }

                /* add the current range and subtract the first range: this is done after the pattern recognition
                 * when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                 */
                nearPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, i - 1) -
                                   CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, nearTrailingIdx - 1);
                i++;
                nearTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }
        
        public static int TasukiGapLookback() => CandleAvgPeriod(CandleSettingType.Near) + 2;
    }
}
