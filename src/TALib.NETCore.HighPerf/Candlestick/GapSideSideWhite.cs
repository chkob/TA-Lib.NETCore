using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode GapSideSideWhite(
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

            int lookbackTotal = GapSideSideWhiteLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            double nearPeriodTotal = default;
            double equalPeriodTotal = default;
            int nearTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.Near);
            int equalTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.Equal);
            int i = nearTrailingIdx;
            while (i < startIdx)
            {
                nearPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, i - 1);
                i++;
            }

            i = equalTrailingIdx;
            while (i < startIdx)
            {
                equalPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, i - 1);
                i++;
            }

            i = startIdx;

            int outIdx = default;
            do
            {
                if (( // upside or downside gap between the 1st candle and both the next 2 candles
                        RealBodyGapUp(ref inOpen, ref inClose, i - 1, i - 2) && RealBodyGapUp(ref inOpen, ref inClose, i, i - 2)
                        ||
                        RealBodyGapDown(ref inOpen, ref inClose, i - 1, i - 2) && RealBodyGapDown(ref inOpen, ref inClose, i, i - 2)
                    ) &&
                    CandleColor(ref inClose, ref inOpen, i - 1) && // 2nd: white
                    CandleColor(ref inClose, ref inOpen, i) && // 3rd: white
                    RealBody(ref inClose, ref inOpen, i) >= RealBody(ref inClose, ref inOpen, i - 1) -
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, nearPeriodTotal, i - 1) && // same size 2 and 3
                    RealBody(ref inClose, ref inOpen, i) <= RealBody(ref inClose, ref inOpen, i - 1) +
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, nearPeriodTotal, i - 1) &&
                    inOpen[i] >= inOpen[i - 1] -
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, equalPeriodTotal,
                        i - 1) && // same open 2 and 3
                    inOpen[i] <= inOpen[i - 1] +
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, equalPeriodTotal, i - 1))
                {
                    outInteger[outIdx++] = RealBodyGapUp(ref inOpen, ref inClose, i - 1, i - 2) ? 100 : -100;
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
                equalPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, i - 1) -
                                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, equalTrailingIdx - 1);
                i++;
                nearTrailingIdx++;
                equalTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int GapSideSideWhiteLookback() =>
            Math.Max(CandleAvgPeriod(CandleSettingType.Near), CandleAvgPeriod(CandleSettingType.Equal)) + 2;
    }
}
