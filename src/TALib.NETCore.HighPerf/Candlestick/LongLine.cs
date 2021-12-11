using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode LongLine(
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

            int lookbackTotal = LongLineLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            decimal bodyPeriodTotal = default;
            int bodyTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyLong);
            decimal shadowPeriodTotal = default;
            int shadowTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.ShadowShort);
            int i = bodyTrailingIdx;
            while (i < startIdx)
            {
                bodyPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i);
                i++;
            }

            i = shadowTrailingIdx;
            while (i < startIdx)
            {
                shadowPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowShort, i);
                i++;
            }

            int outIdx = default;
            do
            {
                if (RealBody(ref inClose, ref inOpen, i) >
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, bodyPeriodTotal, i) &&
                    UpperShadow(ref inHigh, ref inClose, ref inOpen, i) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowShort, shadowPeriodTotal, i) &&
                    LowerShadow(ref inClose, ref inOpen, ref inLow, i) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowShort, shadowPeriodTotal, i))
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
                bodyPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i) -
                                   CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, bodyTrailingIdx);
                shadowPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowShort, i) -
                                     CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowShort, shadowTrailingIdx);
                i++;
                bodyTrailingIdx++;
                shadowTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int LongLineLookback() =>
            Math.Max(CandleAvgPeriod(CandleSettingType.BodyLong), CandleAvgPeriod(CandleSettingType.ShadowShort));
    }
}
