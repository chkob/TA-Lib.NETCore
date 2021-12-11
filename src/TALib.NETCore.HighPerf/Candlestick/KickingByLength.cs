using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode KickingByLength(
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

            int lookbackTotal = KickingByLengthLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            var shadowVeryShortPeriodTotal = BufferHelpers.New(2);
            var bodyLongPeriodTotal = BufferHelpers.New(2);
            int shadowVeryShortTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.ShadowVeryShort);
            int bodyLongTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyLong);
            int i = shadowVeryShortTrailingIdx;
            while (i < startIdx)
            {
                shadowVeryShortPeriodTotal[1] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i - 1);
                shadowVeryShortPeriodTotal[0] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i);
                i++;
            }

            i = bodyLongTrailingIdx;
            while (i < startIdx)
            {
                bodyLongPeriodTotal[1] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i - 1);
                bodyLongPeriodTotal[0] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i);
                i++;
            }

            i = startIdx;
            int outIdx = default;
            do
            {
                if (CandleColor(ref inClose, ref inOpen, i - 1) == !CandleColor(ref inClose, ref inOpen, i) && // opposite candles
                    // 1st marubozu
                    RealBody(ref inClose, ref inOpen, i - 1) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong,
                        bodyLongPeriodTotal[1], i - 1) &&
                    UpperShadow(ref inHigh, ref inClose, ref inOpen, i - 1) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[1], i - 1) &&
                    LowerShadow(ref inClose, ref inOpen, ref inLow, i - 1) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[1], i - 1) &&
                    // 2nd marubozu
                    RealBody(ref inClose, ref inOpen, i) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong,
                        bodyLongPeriodTotal[0], i) &&
                    UpperShadow(ref inHigh, ref inClose, ref inOpen, i) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[0], i) &&
                    LowerShadow(ref inClose, ref inOpen, ref inLow, i) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[0], i) &&
                    // gap
                    (!CandleColor(ref inClose, ref inOpen, i - 1) && HighPerf.Lib.CandleGapUp(ref inLow, ref inHigh, i, i - 1)
                     ||
                     CandleColor(ref inClose, ref inOpen, i - 1) && HighPerf.Lib.CandleGapDown(ref inLow, ref inHigh, i, i - 1)))
                {
                    outInteger[outIdx++] = Convert.ToInt32(CandleColor(ref inClose, ref inOpen,
                        RealBody(ref inClose, ref inOpen, i) > RealBody(ref inClose, ref inOpen, i - 1) ? i : i - 1)) * 100;
                }
                else
                {
                    outInteger[outIdx++] = 0;
                }

                /* add the current range and subtract the first range: this is done after the pattern recognition
                 * when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                 */
                for (var totIdx = 1; totIdx >= 0; --totIdx)
                {
                    bodyLongPeriodTotal[totIdx] +=
                        CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i - totIdx)
                        - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, bodyLongTrailingIdx - totIdx);
                    shadowVeryShortPeriodTotal[totIdx] +=
                        CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i - totIdx)
                        - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort,
                            shadowVeryShortTrailingIdx - totIdx);
                }

                i++;
                shadowVeryShortTrailingIdx++;
                bodyLongTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int KickingByLengthLookback() =>
            Math.Max(CandleAvgPeriod(CandleSettingType.ShadowVeryShort), CandleAvgPeriod(CandleSettingType.BodyLong)) + 1;
    }
}
