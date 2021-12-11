using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode MatchingLow(
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

            int lookbackTotal = MatchingLowLookback();
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
            int i = equalTrailingIdx;
            while (i < startIdx)
            {
                equalPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, i - 1);
                i++;
            }

            i = startIdx;
            int outIdx = default;
            do
            {
                if (!CandleColor(ref inClose, ref inOpen, i - 1) && // first black
                    !CandleColor(ref inClose, ref inOpen, i) && // second black
                    inClose[i] <= inClose[i - 1] +
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, equalPeriodTotal,
                        i - 1) && // 1st and 2nd same close
                    inClose[i] >= inClose[i - 1] -
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, equalPeriodTotal, i - 1)
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
                equalPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, i - 1) -
                                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, equalTrailingIdx - 1);
                i++;
                equalTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int MatchingLowLookback() => CandleAvgPeriod(CandleSettingType.Equal) + 1;
    }
}
