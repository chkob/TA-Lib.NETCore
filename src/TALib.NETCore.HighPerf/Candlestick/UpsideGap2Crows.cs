using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode UpsideGap2Crows(
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

            int lookbackTotal = UpsideGap2CrowsLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            decimal bodyLongPeriodTotal = default;
            decimal bodyShortPeriodTotal = default;
            int bodyLongTrailingIdx = startIdx - 2 - CandleAvgPeriod(CandleSettingType.BodyLong);
            int bodyShortTrailingIdx = startIdx - 1 - CandleAvgPeriod(CandleSettingType.BodyShort);
            int i = bodyLongTrailingIdx;
            while (i < startIdx - 2)
            {
                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i);
                i++;
            }

            i = bodyShortTrailingIdx;
            while (i < startIdx - 1)
            {
                bodyShortPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i);
                i++;
            }

            i = startIdx;
            int outIdx = default;
            do
            {
                if (CandleColor(ref inClose, ref inOpen, i - 2) && // 1st: white
                    RealBody(ref inClose, ref inOpen, i - 2) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong,
                        bodyLongPeriodTotal, i - 2) && //      long
                    !CandleColor(ref inClose, ref inOpen, i - 1) && // 2nd: black
                    RealBody(ref inClose, ref inOpen, i - 1) <= CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort,
                        bodyShortPeriodTotal, i - 1) && //      short
                    RealBodyGapUp(ref inOpen, ref inClose, i - 1, i - 2) && //      gapping up
                    !CandleColor(ref inClose, ref inOpen, i) && // 3rd: black
                    inOpen[i] > inOpen[i - 1] && inClose[i] < inClose[i - 1] && // 3rd: engulfing prior rb
                    inClose[i] > inClose[i - 2] //      closing above 1st
                )
                {
                    outInteger[outIdx++] = -100;
                }
                else
                {
                    outInteger[outIdx++] = 0;
                }

                /* add the current range and subtract the first range: this is done after the pattern recognition
                 * when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                 */
                bodyLongPeriodTotal +=
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i - 2) -
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, bodyLongTrailingIdx);
                bodyShortPeriodTotal +=
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i - 1) -
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, bodyShortTrailingIdx);
                i++;
                bodyLongTrailingIdx++;
                bodyShortTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int UpsideGap2CrowsLookback() =>
            Math.Max(CandleAvgPeriod(CandleSettingType.BodyShort), CandleAvgPeriod(CandleSettingType.BodyLong)) + 2;
    }
}
