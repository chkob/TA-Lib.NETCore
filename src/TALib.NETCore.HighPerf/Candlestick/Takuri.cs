using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Takuri(
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

            int lookbackTotal = TakuriLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            decimal bodyDojiPeriodTotal = default;
            int bodyDojiTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyDoji);
            decimal shadowVeryShortPeriodTotal = default;
            int shadowVeryShortTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.ShadowVeryShort);
            decimal shadowVeryLongPeriodTotal = default;
            int shadowVeryLongTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.ShadowVeryLong);
            int i = bodyDojiTrailingIdx;
            while (i < startIdx)
            {
                bodyDojiPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji, i);
                i++;
            }

            i = shadowVeryShortTrailingIdx;
            while (i < startIdx)
            {
                shadowVeryShortPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i);
                i++;
            }

            i = shadowVeryLongTrailingIdx;
            while (i < startIdx)
            {
                shadowVeryLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryLong, i);
                i++;
            }

            int outIdx = default;
            do
            {
                if (RealBody(ref inClose, ref inOpen, i) <=
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji, bodyDojiPeriodTotal, i) &&
                    UpperShadow(ref inHigh, ref inClose, ref inOpen, i) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal, i) &&
                    LowerShadow(ref inClose, ref inOpen, ref inLow, i) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowVeryShort, shadowVeryLongPeriodTotal, i)
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
                bodyDojiPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji, i) -
                                       CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji, bodyDojiTrailingIdx);
                shadowVeryShortPeriodTotal +=
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i)
                    - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, shadowVeryShortTrailingIdx);
                shadowVeryLongPeriodTotal +=
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryLong, i)
                    - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryLong, shadowVeryLongTrailingIdx);
                i++;
                bodyDojiTrailingIdx++;
                shadowVeryShortTrailingIdx++;
                shadowVeryLongTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int TakuriLookback() =>
            Math.Max(
                Math.Max(CandleAvgPeriod(CandleSettingType.BodyDoji), CandleAvgPeriod(CandleSettingType.ShadowVeryShort)),
                CandleAvgPeriod(CandleSettingType.ShadowVeryLong));
    }
}
