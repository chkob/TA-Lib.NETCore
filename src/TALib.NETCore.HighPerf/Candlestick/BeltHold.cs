using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode BeltHold(
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

            int lookbackTotal = BeltHoldLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            decimal bodyLongPeriodTotal = default;
            int bodyLongTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyLong);
            decimal shadowVeryShortPeriodTotal = default;
            int shadowVeryShortTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.ShadowVeryShort);
            int i = bodyLongTrailingIdx;
            while (i < startIdx)
            {
                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i);
                i++;
            }

            i = shadowVeryShortTrailingIdx;
            while (i < startIdx)
            {
                shadowVeryShortPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i);
                i++;
            }

            int outIdx = default;
            do
            {
                if (RealBody(ref inClose, ref inOpen, i) >
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, bodyLongPeriodTotal, i) && // long body
                    // white body and very short lower shadow
                    (CandleColor(ref inClose, ref inOpen, i) &&
                     LowerShadow(ref inClose, ref inOpen, ref inLow, i) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                         CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal, i)
                     ||
                     // black body and very short upper shadow
                     !CandleColor(ref inClose, ref inOpen, i) &&
                     UpperShadow(ref inHigh, ref inClose, ref inOpen, i) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                         CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal, i)))
                {
                    outInteger[outIdx++] = Convert.ToInt32(CandleColor(ref inClose, ref inOpen, i)) * 100;
                }
                else
                {
                    outInteger[outIdx++] = 0;
                }

                /* add the current range and subtract the first range: this is done after the pattern recognition
                 * when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                 */
                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i) -
                                       CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, bodyLongTrailingIdx);
                shadowVeryShortPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i)
                                              - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort,
                                                  shadowVeryShortTrailingIdx);
                i++;
                bodyLongTrailingIdx++;
                shadowVeryShortTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int BeltHoldLookback() =>
            Math.Max(CandleAvgPeriod(CandleSettingType.BodyLong), CandleAvgPeriod(CandleSettingType.ShadowVeryShort));
    }
}
