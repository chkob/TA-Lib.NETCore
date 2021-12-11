using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode HaramiCross(
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

            int lookbackTotal = HaramiCrossLookback();
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
            int bodyLongTrailingIdx = startIdx - 1 - CandleAvgPeriod(CandleSettingType.BodyLong);
            int bodyDojiTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyDoji);
            int i = bodyLongTrailingIdx;
            while (i < startIdx - 1)
            {
                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i);
                i++;
            }

            i = bodyDojiTrailingIdx;
            while (i < startIdx)
            {
                bodyDojiPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji, i);
                i++;
            }

            i = startIdx;

            int outIdx = default;
            do
            {
                if (RealBody(ref inClose, ref inOpen, i - 1) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong,
                        bodyLongPeriodTotal, i - 1) && // 1st: long
                    RealBody(ref inClose, ref inOpen, i) <= CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji,
                        bodyDojiPeriodTotal, i)) // 2nd: doji
                {
                    if (Math.Max(inClose[i], inOpen[i]) < Math.Max(inClose[i - 1], inOpen[i - 1]) && // 2nd is engulfed by 1st
                        Math.Min(inClose[i], inOpen[i]) > Math.Min(inClose[i - 1], inOpen[i - 1]))
                    {
                        outInteger[outIdx++] = Convert.ToInt32(!CandleColor(ref inClose, ref inOpen, i - 1)) * 100;
                    }
                    else if (Math.Max(inClose[i], inOpen[i]) <= Math.Max(inClose[i - 1], inOpen[i - 1]) && // 2nd is engulfed by 1st
                             Math.Min(inClose[i], inOpen[i]) >= Math.Min(inClose[i - 1], inOpen[i - 1])) // (one end of real body can match;
                    {
                        outInteger[outIdx++] = Convert.ToInt32(!CandleColor(ref inClose, ref inOpen, i - 1)) * 80;
                    }
                    else
                    {
                        outInteger[outIdx++] = 0;
                    }
                }
                else
                {
                    outInteger[outIdx++] = 0;
                }

                /* add the current range and subtract the first range: this is done after the pattern recognition
                 * when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                 */
                bodyLongPeriodTotal +=
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i - 1) -
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, bodyLongTrailingIdx);
                bodyDojiPeriodTotal +=
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji, i) -
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji, bodyDojiTrailingIdx);
                i++;
                bodyLongTrailingIdx++;
                bodyDojiTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int HaramiCrossLookback() =>
            Math.Max(CandleAvgPeriod(CandleSettingType.BodyDoji), CandleAvgPeriod(CandleSettingType.BodyLong)) + 1;
    }
}
