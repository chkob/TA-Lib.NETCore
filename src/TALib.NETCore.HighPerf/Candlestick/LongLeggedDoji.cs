using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode LongLeggedDoji(
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

            int lookbackTotal = LongLeggedDojiLookback();
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
            decimal shadowLongPeriodTotal = default;
            int shadowLongTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.ShadowLong);
            int i = bodyDojiTrailingIdx;
            while (i < startIdx)
            {
                bodyDojiPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji, i);
                i++;
            }

            i = shadowLongTrailingIdx;
            while (i < startIdx)
            {
                shadowLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowLong, i);
                i++;
            }

            int outIdx = default;
            do
            {
                if (RealBody(ref inClose, ref inOpen, i) <=
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji, bodyDojiPeriodTotal, i) &&
                    (LowerShadow(ref inClose, ref inOpen, ref inLow, i) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                         CandleSettingType.ShadowLong, shadowLongPeriodTotal, i)
                     ||
                     UpperShadow(ref inHigh, ref inClose, ref inOpen, i) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                         CandleSettingType.ShadowLong, shadowLongPeriodTotal, i)))
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
                shadowLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowLong, i) -
                                         CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowLong,
                                             shadowLongTrailingIdx);
                i++;
                bodyDojiTrailingIdx++;
                shadowLongTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int LongLeggedDojiLookback() =>
            Math.Max(CandleAvgPeriod(CandleSettingType.BodyDoji), CandleAvgPeriod(CandleSettingType.ShadowLong));
    }
}
