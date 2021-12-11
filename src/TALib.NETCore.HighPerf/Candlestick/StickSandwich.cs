using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode StickSandwich(
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

            int lookbackTotal = StickSandwichLookback();
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
                equalPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, i - 2);
                i++;
            }

            i = startIdx;
            int outIdx = default;
            do
            {
                if (!CandleColor(ref inClose, ref inOpen, i - 2) && // first black
                    CandleColor(ref inClose, ref inOpen, i - 1) && // second white
                    !CandleColor(ref inClose, ref inOpen, i) && // third black
                    inLow[i - 1] > inClose[i - 2] && // 2nd low > prior close
                    inClose[i] <= inClose[i - 2] +
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, equalPeriodTotal,
                        i - 2) && // 1st and 3rd same close
                    inClose[i] >= inClose[i - 2] -
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, equalPeriodTotal, i - 2)
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
                equalPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, i - 2) -
                                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Equal, equalTrailingIdx - 2);
                i++;
                equalTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int StickSandwichLookback() => CandleAvgPeriod(CandleSettingType.Equal) + 2;
    }
}
