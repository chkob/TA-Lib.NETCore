using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode EveningStar(
            ref Span<decimal> inOpen,
            ref Span<decimal> inHigh,
            ref Span<decimal> inLow,
            ref Span<decimal> inClose,
            int startIdx,
            int endIdx,
            int[] outInteger,
            out int outBegIdx,
            out int outNbElement,
            decimal optInPenetration = 0.3m)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inOpen == null || inHigh == null || inLow == null || inClose == null || outInteger == null || optInPenetration < 0.0m)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = EveningStarLookback();
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
            decimal bodyShortPeriodTotal2 = default;
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
                bodyShortPeriodTotal2 += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i + 1);
                i++;
            }

            i = startIdx;

            int outIdx = default;
            do
            {
                if (RealBody(ref inClose, ref inOpen, i - 2) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong,
                        bodyLongPeriodTotal, i - 2) && // 1st: long
                    CandleColor(ref inClose, ref inOpen, i - 2) && //           white
                    RealBody(ref inClose, ref inOpen, i - 1) <= CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort,
                        bodyShortPeriodTotal, i - 1) && // 2nd: short
                    RealBodyGapUp(ref inOpen, ref inClose, i - 1, i - 2) && //            gapping up
                    RealBody(ref inClose, ref inOpen, i) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort,
                        bodyShortPeriodTotal2, i) && // 3rd: longer than short
                    !CandleColor(ref inClose, ref inOpen, i) && //          black real body
                    inClose[i] < inClose[i - 2] - RealBody(ref inClose, ref inOpen, i - 2) * optInPenetration) //       well within 1st rb
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
                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i - 2) -
                                       CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, bodyLongTrailingIdx);
                bodyShortPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i - 1) -
                                        CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, bodyShortTrailingIdx);
                bodyShortPeriodTotal2 += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i) -
                                         CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort,
                                             bodyShortTrailingIdx + 1);
                i++;
                bodyLongTrailingIdx++;
                bodyShortTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int EveningStarLookback() =>
            Math.Max(CandleAvgPeriod(CandleSettingType.BodyShort), CandleAvgPeriod(CandleSettingType.BodyLong)) + 2;
    }
}
