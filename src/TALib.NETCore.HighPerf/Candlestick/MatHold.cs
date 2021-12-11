using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode MatHold(
            ref Span<decimal> inOpen,
            ref Span<decimal> inHigh,
            ref Span<decimal> inLow,
            ref Span<decimal> inClose,
            int startIdx,
            int endIdx,
            int[] outInteger,
            out int outBegIdx,
            out int outNbElement,
            decimal optInPenetration = 0.5m)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inOpen == null || inHigh == null || inLow == null || inClose == null || outInteger == null || optInPenetration <  0.0m)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = MatHoldLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            var bodyPeriodTotal = BufferHelpers.New(5);
            int bodyShortTrailingIdx = startIdx - HighPerf.Lib.Globals.CandleSettings[(int) CandleSettingType.BodyShort].AvgPeriod;
            int bodyLongTrailingIdx = startIdx - HighPerf.Lib.Globals.CandleSettings[(int) CandleSettingType.BodyLong].AvgPeriod;
            int i = bodyShortTrailingIdx;
            while (i < startIdx)
            {
                bodyPeriodTotal[3] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i - 3);
                bodyPeriodTotal[2] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i - 2);
                bodyPeriodTotal[1] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i - 1);
                i++;
            }

            i = bodyLongTrailingIdx;
            while (i < startIdx)
            {
                bodyPeriodTotal[4] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i - 4);
                i++;
            }

            i = startIdx;
            int outIdx = default;
            do
            {
                if ( // 1st long, then 3 small
                    RealBody(ref inClose, ref inOpen, i - 4) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong,
                        bodyPeriodTotal[4], i - 4) &&
                    RealBody(ref inClose, ref inOpen, i - 3) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort,
                        bodyPeriodTotal[3], i - 3) &&
                    RealBody(ref inClose, ref inOpen, i - 2) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort,
                        bodyPeriodTotal[2], i - 2) &&
                    RealBody(ref inClose, ref inOpen, i - 1) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort,
                        bodyPeriodTotal[1], i - 1) &&
                    // white, black, 2 black or white, white
                    CandleColor(ref inClose, ref inOpen, i - 4) &&
                    !CandleColor(ref inClose, ref inOpen, i - 3) &&
                    CandleColor(ref inClose, ref inOpen, i) &&
                    // upside gap 1st to 2nd
                    RealBodyGapUp(ref inOpen, ref inClose, i - 3, i - 4) &&
                    // 3rd to 4th hold within 1st: a part of the real body must be within 1st real body
                    Math.Min(inOpen[i - 2], inClose[i - 2]) < inClose[i - 4] &&
                    Math.Min(inOpen[i - 1], inClose[i - 1]) < inClose[i - 4] &&
                    // reaction days penetrate first body less than optInPenetration percent
                    Math.Min(inOpen[i - 2], inClose[i - 2]) > inClose[i - 4] - RealBody(ref inClose, ref inOpen, i - 4) * optInPenetration &&
                    Math.Min(inOpen[i - 1], inClose[i - 1]) > inClose[i - 4] - RealBody(ref inClose, ref inOpen, i - 4) * optInPenetration &&
                    // 2nd to 4th are falling
                    Math.Max(inClose[i - 2], inOpen[i - 2]) < inOpen[i - 3] &&
                    Math.Max(inClose[i - 1], inOpen[i - 1]) < Math.Max(inClose[i - 2], inOpen[i - 2]) &&
                    // 5th opens above the prior close
                    inOpen[i] > inClose[i - 1] &&
                    // 5th closes above the highest high of the reaction days
                    inClose[i] > Math.Max(Math.Max(inHigh[i - 3], inHigh[i - 2]), inHigh[i - 1])
                )
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
                bodyPeriodTotal[4] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i - 4) -
                                      CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, bodyLongTrailingIdx - 4);
                for (var totIdx = 3; totIdx >= 1; --totIdx)
                {
                    bodyPeriodTotal[totIdx] +=
                        CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i - totIdx)
                        - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, bodyShortTrailingIdx - totIdx);
                }

                i++;
                bodyShortTrailingIdx++;
                bodyLongTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int MatHoldLookback() =>
            Math.Max(CandleAvgPeriod(CandleSettingType.BodyShort), CandleAvgPeriod(CandleSettingType.ShadowLong)) + 4;
    }
}
