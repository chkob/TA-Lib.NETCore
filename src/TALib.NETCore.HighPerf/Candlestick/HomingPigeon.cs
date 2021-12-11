using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode HomingPigeon(
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

            int lookbackTotal = HomingPigeonLookback();
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
            int bodyLongTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyLong);
            int bodyShortTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyShort);
            int i = bodyLongTrailingIdx;
            while (i < startIdx)
            {
                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i - 1);
                i++;
            }

            i = bodyShortTrailingIdx;
            while (i < startIdx)
            {
                bodyShortPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i);
                i++;
            }

            i = startIdx;
            int outIdx = default;
            do
            {
                if (!CandleColor(ref inClose, ref inOpen, i - 1) && // 1st black
                    !CandleColor(ref inClose, ref inOpen, i) && // 2nd black
                    RealBody(ref inClose, ref inOpen, i - 1) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong,
                        bodyLongPeriodTotal, i - 1) && // 1st long
                    RealBody(ref inClose, ref inOpen, i) <= CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort,
                        bodyShortPeriodTotal, i) && // 2nd short
                    inOpen[i] < inOpen[i - 1] && // 2nd engulfed by 1st
                    inClose[i] > inClose[i - 1])
                {
                    outInteger[outIdx++] = 100;
                }
                else
                {
                    outInteger[outIdx++] = 0;
                }

                /* add the current range and subtract the first range: this is done after the pattern recognition
                 * when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                 */
                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i - 1) -
                                       CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, bodyLongTrailingIdx - 1);
                bodyShortPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i) -
                                        CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, bodyShortTrailingIdx);
                i++;
                bodyLongTrailingIdx++;
                bodyShortTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int HomingPigeonLookback() =>
            Math.Max(CandleAvgPeriod(CandleSettingType.BodyShort), CandleAvgPeriod(CandleSettingType.BodyLong)) + 1;
    }
}
