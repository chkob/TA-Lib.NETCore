using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode HangingMan(
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

            int lookbackTotal = HangingManLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            decimal bodyPeriodTotal = default;
            int bodyTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyShort);
            decimal shadowLongPeriodTotal = default;
            int shadowLongTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyLong);
            decimal shadowVeryShortPeriodTotal = default;
            int shadowVeryShortTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.ShadowVeryShort);
            decimal nearPeriodTotal = default;
            int nearTrailingIdx = startIdx - 1 - CandleAvgPeriod(CandleSettingType.Near);
            int i = bodyTrailingIdx;
            while (i < startIdx)
            {
                bodyPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i);
                i++;
            }

            i = shadowLongTrailingIdx;
            while (i < startIdx)
            {
                shadowLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowLong, i);
                i++;
            }

            i = shadowVeryShortTrailingIdx;
            while (i < startIdx)
            {
                shadowVeryShortPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i);
                i++;
            }

            i = nearTrailingIdx;
            while (i < startIdx - 1)
            {
                nearPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, i);
                i++;
            }

            i = startIdx;

            int outIdx = default;
            do
            {
                if (RealBody(ref inClose, ref inOpen, i) <
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, bodyPeriodTotal, i) && // small rb
                    LowerShadow(ref inClose, ref inOpen, ref inLow, i) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowLong, shadowLongPeriodTotal, i) && // long lower shadow
                    UpperShadow(ref inHigh, ref inClose, ref inOpen, i) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal, i) && // very short upper shadow
                    Math.Min(inClose[i], inOpen[i]) >= inHigh[i - 1] -
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, nearPeriodTotal,
                        i - 1) // rb near the prior candle's highs
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
                bodyPeriodTotal +=
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i)
                    - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, bodyTrailingIdx);
                shadowLongPeriodTotal +=
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowLong, i)
                    - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowLong, shadowLongTrailingIdx);
                shadowVeryShortPeriodTotal +=
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i)
                    - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, shadowVeryShortTrailingIdx);
                nearPeriodTotal +=
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, i - 1)
                    - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, nearTrailingIdx);
                i++;
                bodyTrailingIdx++;
                shadowLongTrailingIdx++;
                shadowVeryShortTrailingIdx++;
                nearTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int HangingManLookback() =>
            Math.Max(
                Math.Max(
                    Math.Max(CandleAvgPeriod(CandleSettingType.BodyShort), CandleAvgPeriod(CandleSettingType.ShadowLong)),
                    CandleAvgPeriod(CandleSettingType.ShadowVeryShort)),
                CandleAvgPeriod(CandleSettingType.Near)
            ) + 1;
    }
}
