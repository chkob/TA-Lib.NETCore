using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode ThreeBlackCrows(
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

            int lookbackTotal = ThreeBlackCrowsLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            var shadowVeryShortPeriodTotal = BufferHelpers.New(3);
            int shadowVeryShortTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.ShadowVeryShort);
            int i = shadowVeryShortTrailingIdx;
            while (i < startIdx)
            {
                shadowVeryShortPeriodTotal[2] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i - 2);
                shadowVeryShortPeriodTotal[1] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i - 1);
                shadowVeryShortPeriodTotal[0] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i);
                i++;
            }

            i = startIdx;

            int outIdx = default;
            do
            {
                if (CandleColor(ref inClose, ref inOpen, i - 3) &&
                    !CandleColor(ref inClose, ref inOpen, i - 2) &&
                    LowerShadow(ref inClose, ref inOpen, ref inLow, i - 2) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[2], i - 2) &&
                    !CandleColor(ref inClose, ref inOpen, i - 1) &&
                    LowerShadow(ref inClose, ref inOpen, ref inLow, i - 1) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[1], i - 1) &&
                    !CandleColor(ref inClose, ref inOpen, i) &&
                    LowerShadow(ref inClose, ref inOpen, ref inLow, i) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[0], i) &&
                    inOpen[i - 1] < inOpen[i - 2] && inOpen[i - 1] > inClose[i - 2] &&
                    inOpen[i] < inOpen[i - 1] && inOpen[i] > inClose[i - 1] &&
                    inHigh[i - 3] > inClose[i - 2] &&
                    inClose[i - 2] > inClose[i - 1] &&
                    inClose[i - 1] > inClose[i])
                {
                    outInteger[outIdx++] = -100;
                }
                else
                {
                    outInteger[outIdx++] = 0;
                }

                for (var totIdx = 2; totIdx >= 0; --totIdx)
                {
                    shadowVeryShortPeriodTotal[totIdx] +=
                        CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i - totIdx)
                        - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort,
                            shadowVeryShortTrailingIdx - totIdx);
                }

                i++;
                shadowVeryShortTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }
        
        public static int ThreeBlackCrowsLookback() => CandleAvgPeriod(CandleSettingType.ShadowVeryShort) + 3;
    }
}
