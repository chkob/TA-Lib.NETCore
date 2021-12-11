using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode OnNeck(
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

            int lookbackTotal = OnNeckLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            decimal equalPeriodTotal = default;
            int equalTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.Equal);
            decimal bodyLongPeriodTotal = default;
            int bodyLongTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyLong);
            int i = equalTrailingIdx;
            while (i < startIdx)
            {
                equalPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, i - 1);
                i++;
            }

            i = bodyLongTrailingIdx;
            while (i < startIdx)
            {
                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i - 1);
                i++;
            }

            i = startIdx;
            int outIdx = default;
            do
            {
                if (!CandleColor(ref inClose, ref inOpen, i - 1) && // 1st: black
                    RealBody(ref inClose, ref inOpen, i - 1) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong,
                        bodyLongPeriodTotal, i - 1) && //      long
                    CandleColor(ref inClose, ref inOpen, i) && // 2nd: white
                    inOpen[i] < inLow[i - 1] && //   open below prior low
                    inClose[i] <= inLow[i - 1] +
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, equalPeriodTotal,
                        i - 1) && //   close equal to prior low
                    inClose[i] >= inLow[i - 1] -
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, equalPeriodTotal, i - 1)
                )
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
                equalPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, i - 1) -
                                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, equalTrailingIdx - 1);
                bodyLongPeriodTotal +=
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i - 1)
                    - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, bodyLongTrailingIdx - 1);
                i++;
                equalTrailingIdx++;
                bodyLongTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int OnNeckLookback() =>
            Math.Max(CandleAvgPeriod(CandleSettingType.Equal), CandleAvgPeriod(CandleSettingType.BodyLong)) + 1;
    }
}
