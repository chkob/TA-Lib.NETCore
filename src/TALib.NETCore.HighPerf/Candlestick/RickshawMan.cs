using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode RickshawMan(
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

            int lookbackTotal = RickshawManLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            double bodyDojiPeriodTotal = default;
            int bodyDojiTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyDoji);
            double shadowLongPeriodTotal = default;
            int shadowLongTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.ShadowLong);
            double nearPeriodTotal = default;
            int nearTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.Near);
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

            i = nearTrailingIdx;
            while (i < startIdx)
            {
                nearPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, i);
                i++;
            }

            int outIdx = default;
            do
            {
                if (RealBody(ref inClose, ref inOpen, i) <=
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji, bodyDojiPeriodTotal, i) && // doji
                    LowerShadow(ref inClose, ref inOpen, ref inLow, i) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowLong, shadowLongPeriodTotal, i) && // long shadow
                    UpperShadow(ref inClose, ref inOpen, ref inLow, i) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowLong, shadowLongPeriodTotal, i) && Math.Min(inOpen[i], inClose[i]) <= inLow[i] +
                    HighPerf.Lib.HighLowRange(ref inHigh, ref inLow, i) / 2 + CandleAverage(ref inOpen,
                        ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, nearPeriodTotal, i) && Math.Max(inOpen[i], inClose[i]) >= inLow[i] +
                    HighPerf.Lib.HighLowRange(ref inHigh, ref inLow, i) / 2 - CandleAverage(ref inOpen,
                        ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, nearPeriodTotal, i))
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
                shadowLongPeriodTotal +=
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowLong, i) -
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowLong, shadowLongTrailingIdx);
                nearPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, i) -
                                   CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, nearTrailingIdx);

                i++;
                bodyDojiTrailingIdx++;
                shadowLongTrailingIdx++;
                nearTrailingIdx++;
            } while (i <= endIdx);


            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int RickshawManLookback() =>
            Math.Max(
                Math.Max(CandleAvgPeriod(CandleSettingType.BodyDoji), CandleAvgPeriod(CandleSettingType.ShadowLong)),
                CandleAvgPeriod(CandleSettingType.Near));
    }
}
