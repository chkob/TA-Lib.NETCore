using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode ShootingStar(
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

            int lookbackTotal = ShootingStarLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            double bodyPeriodTotal = default;
            int bodyTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyShort);
            double shadowLongPeriodTotal = default;
            int shadowLongTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.ShadowLong);
            double shadowVeryShortPeriodTotal = default;
            int shadowVeryShortTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.ShadowVeryShort);
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

            int outIdx = default;
            do
            {
                if (RealBody(ref inClose, ref inOpen, i) <
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, bodyPeriodTotal, i) && // small rb
                    UpperShadow(ref inHigh, ref inClose, ref inOpen, i) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowLong, shadowLongPeriodTotal, i) && // long upper shadow
                    LowerShadow(ref inClose, ref inOpen, ref inLow, i) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal, i) && // very short lower shadow
                    RealBodyGapUp(ref inOpen, ref inClose, i, i - 1)) // gap up
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
                i++;
                bodyTrailingIdx++;
                shadowLongTrailingIdx++;
                shadowVeryShortTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int ShootingStarLookback() =>
            Math.Max(
                Math.Max(CandleAvgPeriod(CandleSettingType.BodyShort), CandleAvgPeriod(CandleSettingType.ShadowLong)),
                CandleAvgPeriod(CandleSettingType.ShadowVeryShort)
            ) + 1;
    }
}
