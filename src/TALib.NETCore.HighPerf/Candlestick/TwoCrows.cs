using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode TwoCrows(
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

            int lookbackTotal = TwoCrowsLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            decimal bodyLongPeriodTotal = default;
            int bodyLongTrailingIdx = startIdx - 2 - CandleAvgPeriod(CandleSettingType.BodyLong);
            int i = bodyLongTrailingIdx;
            while (i < startIdx - 2)
            {
                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i);
                i++;
            }

            i = startIdx;

            int outIdx = default;
            do
            {
                if (CandleColor(ref inClose, ref inOpen, i - 2) &&
                    RealBody(ref inClose, ref inOpen, i - 2) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong,
                        bodyLongPeriodTotal, i) &&
                    !CandleColor(ref inClose, ref inOpen, i - 1) &&
                    RealBodyGapUp(ref inOpen, ref inClose, i - 1, i - 2) &&
                    !CandleColor(ref inClose, ref inOpen, i) &&
                    inOpen[i] < inOpen[i - 1] && inOpen[i] > inClose[i - 1] &&
                    inClose[i] > inOpen[i - 2] && inClose[i] < inClose[i - 2])
                {
                    outInteger[outIdx++] = -100;
                }
                else
                {
                    outInteger[outIdx++] = 0;
                }

                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i - 2) -
                                       CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, bodyLongTrailingIdx);
                i++;
                bodyLongPeriodTotal++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }
        
        public static int TwoCrowsLookback() => CandleAvgPeriod(CandleSettingType.BodyLong) + 2;
    }
}
