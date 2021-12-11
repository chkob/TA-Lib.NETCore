using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode MorningDojiStar(
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

            int lookbackTotal = MorningDojiStarLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            decimal bodyLongPeriodTotal = default;
            decimal bodyDojiPeriodTotal = default;
            decimal bodyShortPeriodTotal = default;
            int bodyLongTrailingIdx = startIdx - 2 - CandleAvgPeriod(CandleSettingType.BodyLong);
            int bodyDojiTrailingIdx = startIdx - 1 - CandleAvgPeriod(CandleSettingType.BodyDoji);
            int bodyShortTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyShort);
            int i = bodyLongTrailingIdx;
            while (i < startIdx - 2)
            {
                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i);
                i++;
            }

            i = bodyDojiTrailingIdx;
            while (i < startIdx - 1)
            {
                bodyDojiPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji, i);
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
                if (RealBody(ref inClose, ref inOpen, i - 2) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong,
                        bodyLongPeriodTotal, i - 2) && // 1st: long
                    !CandleColor(ref inClose, ref inOpen, i - 2) && //           black
                    RealBody(ref inClose, ref inOpen, i - 1) <= CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji,
                        bodyDojiPeriodTotal, i - 1) && // 2nd: doji
                    RealBodyGapDown(ref inOpen, ref inClose, i - 1, i - 2) && //           gapping down
                    RealBody(ref inClose, ref inOpen, i) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort,
                        bodyShortPeriodTotal, i) && // 3rd: longer than short
                    CandleColor(ref inClose, ref inOpen, i) && //          white real body
                    inClose[i] > inClose[i - 2] +
                    RealBody(ref inClose, ref inOpen, i - 2) * optInPenetration) //               closing well within 1st rb
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
                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i - 2) -
                                       CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, bodyLongTrailingIdx);
                bodyDojiPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji, i - 1) -
                                       CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji, bodyDojiTrailingIdx);
                bodyShortPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i) -
                                        CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, bodyShortTrailingIdx);
                i++;
                bodyLongTrailingIdx++;
                bodyDojiTrailingIdx++;
                bodyShortTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int MorningDojiStarLookback() =>
            Math.Max(
                Math.Max(CandleAvgPeriod(CandleSettingType.BodyDoji), CandleAvgPeriod(CandleSettingType.BodyLong)),
                CandleAvgPeriod(CandleSettingType.BodyShort)
            ) + 2;
    }
}
