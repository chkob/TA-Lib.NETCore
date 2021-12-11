using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode RiseFall3Methods(
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

            int lookbackTotal = RiseFall3MethodsLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            var bodyPeriodTotal = BufferHelpers.New(5);
            int bodyShortTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyShort);
            int bodyLongTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyLong);
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
                bodyPeriodTotal[0] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i);
                i++;
            }

            i = startIdx;
            int outIdx = default;
            do
            {
                if ( // 1st long, then 3 small, 5th long
                    RealBody(ref inClose, ref inOpen, i - 4) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong,
                        bodyPeriodTotal[4], i - 4) &&
                    RealBody(ref inClose, ref inOpen, i - 3) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort,
                        bodyPeriodTotal[3], i - 3) &&
                    RealBody(ref inClose, ref inOpen, i - 2) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort,
                        bodyPeriodTotal[2], i - 2) &&
                    RealBody(ref inClose, ref inOpen, i - 1) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort,
                        bodyPeriodTotal[1], i - 1) &&
                    RealBody(ref inClose, ref inOpen, i) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong,
                        bodyPeriodTotal[0], i) &&
                    // white, 3 black, white  ||  black, 3 white, black
                    CandleColor(ref inClose, ref inOpen, i - 4) == !CandleColor(ref inClose, ref inOpen, i - 3) &&
                    CandleColor(ref inClose, ref inOpen, i - 3) == CandleColor(ref inClose, ref inOpen, i - 2) &&
                    CandleColor(ref inClose, ref inOpen, i - 2) == CandleColor(ref inClose, ref inOpen, i - 1) &&
                    CandleColor(ref inClose, ref inOpen, i - 1) == !CandleColor(ref inClose, ref inOpen, i) &&
                    // 2nd to 4th hold within 1st: a part of the real body must be within 1st range
                    Math.Min(inOpen[i - 3], inClose[i - 3]) < inHigh[i - 4] && Math.Max(inOpen[i - 3], inClose[i - 3]) > inLow[i - 4] &&
                    Math.Min(inOpen[i - 2], inClose[i - 2]) < inHigh[i - 4] && Math.Max(inOpen[i - 2], inClose[i - 2]) > inLow[i - 4] &&
                    Math.Min(inOpen[i - 1], inClose[i - 1]) < inHigh[i - 4] && Math.Max(inOpen[i - 1], inClose[i - 1]) > inLow[i - 4] &&
                    // 2nd to 4th are falling (rising)
                    inClose[i - 2] * Convert.ToInt32(CandleColor(ref inClose, ref inOpen, i - 4)) <
                    inClose[i - 3] * Convert.ToInt32(CandleColor(ref inClose, ref inOpen, i - 4)) &&
                    inClose[i - 1] * Convert.ToInt32(CandleColor(ref inClose, ref inOpen, i - 4)) <
                    inClose[i - 2] * Convert.ToInt32(CandleColor(ref inClose, ref inOpen, i - 4)) &&
                    // 5th opens above (below) the prior close
                    inOpen[i] * Convert.ToInt32(CandleColor(ref inClose, ref inOpen, i - 4)) >
                    inClose[i - 1] * Convert.ToInt32(CandleColor(ref inClose, ref inOpen, i - 4)) &&
                    // 5th closes above (below) the 1st close
                    inClose[i] * Convert.ToInt32(CandleColor(ref inClose, ref inOpen, i - 4)) >
                    inClose[i - 4] * Convert.ToInt32(CandleColor(ref inClose, ref inOpen, i - 4))
                )
                {
                    outInteger[outIdx++] = 100 * Convert.ToInt32(CandleColor(ref inClose, ref inOpen, i - 4));
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

                bodyPeriodTotal[0] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i) -
                                      CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, bodyLongTrailingIdx);

                i++;
                bodyShortTrailingIdx++;
                bodyLongTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int RiseFall3MethodsLookback() =>
            Math.Max(CandleAvgPeriod(CandleSettingType.BodyShort), CandleAvgPeriod(CandleSettingType.BodyLong)) + 4;
    }
}
