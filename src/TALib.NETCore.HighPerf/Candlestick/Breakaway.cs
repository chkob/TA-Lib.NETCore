using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Breakaway(
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

            int lookbackTotal = BreakawayLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            decimal bodyLongPeriodTotal = default;
            int bodyLongTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyLong);
            int i = bodyLongTrailingIdx;
            while (i < startIdx)
            {
                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i - 4);
                i++;
            }

            i = startIdx;

            int outIdx = default;
            do
            {
                if (RealBody(ref inClose, ref inOpen, i - 4) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong,
                        bodyLongPeriodTotal, i - 4) && // 1st long
                    CandleColor(ref inClose, ref inOpen, i - 4) ==
                    CandleColor(ref inClose, ref inOpen, i - 3) && // 1st, 2nd, 4th same color, 5th opposite
                    CandleColor(ref inClose, ref inOpen, i - 3) == CandleColor(ref inClose, ref inOpen, i - 1) &&
                    CandleColor(ref inClose, ref inOpen, i - 1) == !CandleColor(ref inClose, ref inOpen, i) &&
                    (!CandleColor(ref inClose, ref inOpen, i - 4) && // when 1st is black:
                     RealBodyGapDown(ref inOpen, ref inClose, i - 3, i - 4) && // 2nd gaps down
                     inHigh[i - 2] < inHigh[i - 3] && inLow[i - 2] < inLow[i - 3] && // 3rd has lower high and low than 2nd
                     inHigh[i - 1] < inHigh[i - 2] && inLow[i - 1] < inLow[i - 2] && // 4th has lower high and low than 3rd
                     inClose[i] > inOpen[i - 3] && inClose[i] < inClose[i - 4] // 5th closes inside the gap
                     ||
                     CandleColor(ref inClose, ref inOpen, i - 4) && // when 1st is white:
                     RealBodyGapUp(ref inClose, ref inOpen, i - 3, i - 4) && // 2nd gaps up
                     inHigh[i - 2] > inHigh[i - 3] && inLow[i - 2] > inLow[i - 3] && // 3rd has higher high and low than 2nd
                     inHigh[i - 1] > inHigh[i - 2] && inLow[i - 1] > inLow[i - 2] && // 4th has higher high and low than 3rd
                     inClose[i] < inOpen[i - 3] && inClose[i] > inClose[i - 4])) // 5th closes inside the gap
                {
                    outInteger[outIdx++] = Convert.ToInt32(CandleColor(ref inClose, ref inOpen, i)) * 100;
                }
                else
                {
                    outInteger[outIdx++] = 0;
                }

                /* add the current range and subtract the first range: this is done after the pattern recognition
                 * when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                 */
                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i - 4)
                                       - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong,
                                           bodyLongTrailingIdx - 4);
                i++;
                bodyLongTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }
        
        public static int BreakawayLookback() => CandleAvgPeriod(CandleSettingType.BodyLong) + 4;
    }
}
