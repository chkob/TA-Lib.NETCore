using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Tristar(
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

            int lookbackTotal = TristarLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            decimal bodyPeriodTotal = default;
            int bodyTrailingIdx = startIdx - 2 - CandleAvgPeriod(CandleSettingType.BodyDoji);
            int i = bodyTrailingIdx;
            while (i < startIdx - 2)
            {
                bodyPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji, i);
                i++;
            }

            i = startIdx;
            int outIdx = default;
            do
            {
                if (RealBody(ref inClose, ref inOpen, i - 2) <=
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji, bodyPeriodTotal, i - 2) && // 1st: doji
                    RealBody(ref inClose, ref inOpen, i - 1) <= CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji,
                        bodyPeriodTotal, i - 2) && // 2nd: doji
                    RealBody(ref inClose, ref inOpen, i) <= CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji,
                        bodyPeriodTotal, i - 2))
                {
                    // 3rd: doji
                    outInteger[outIdx] = 0;
                    if (RealBodyGapUp(ref inOpen, ref inClose, i - 1, i - 2) // 2nd gaps up
                        &&
                        Math.Max(inOpen[i], inClose[i]) < Math.Max(inOpen[i - 1], inClose[i - 1]) // 3rd is not higher than 2nd
                    )
                    {
                        outInteger[outIdx] = -100;
                    }

                    if (RealBodyGapDown(ref inOpen, ref inClose, i - 1, i - 2) // 2nd gaps down
                        &&
                        Math.Min(inOpen[i], inClose[i]) > Math.Min(inOpen[i - 1], inClose[i - 1]) // 3rd is not lower than 2nd
                    )
                    {
                        outInteger[outIdx] = +100;
                    }

                    outIdx++;
                }
                else
                {
                    outInteger[outIdx++] = 0;
                }

                /* add the current range and subtract the first range: this is done after the pattern recognition
                 * when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                 */
                bodyPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji, i - 2) -
                                   CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyDoji, bodyTrailingIdx);
                i++;
                bodyTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int TristarLookback() => CandleAvgPeriod(CandleSettingType.BodyDoji) + 2;
    }
}
