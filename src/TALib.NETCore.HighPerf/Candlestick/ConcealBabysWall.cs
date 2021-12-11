using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode ConcealBabysWall(
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

            int lookbackTotal = ConcealBabysWallLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            var shadowVeryShortPeriodTotal = BufferHelpers.New(4);
            int shadowVeryShortTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.ShadowVeryShort);
            int i = shadowVeryShortTrailingIdx;
            while (i < startIdx)
            {
                shadowVeryShortPeriodTotal[3] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inOpen, CandleSettingType.ShadowVeryShort, i - 3);
                shadowVeryShortPeriodTotal[2] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inOpen, CandleSettingType.ShadowVeryShort, i - 2);
                shadowVeryShortPeriodTotal[1] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inOpen, CandleSettingType.ShadowVeryShort, i - 1);
                i++;
            }

            i = startIdx;

            int outIdx = default;
            do
            {
                if (!CandleColor(ref inClose, ref inOpen, i - 3) && // 1st black
                    !CandleColor(ref inClose, ref inOpen, i - 2) && // 2nd black
                    !CandleColor(ref inClose, ref inOpen, i - 1) && // 3rd black
                    !CandleColor(ref inClose, ref inOpen, i) && // 4th black
                    // 1st: marubozu
                    LowerShadow(ref inClose, ref inOpen, ref inLow, i - 3) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inOpen,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[3], i - 3) &&
                    UpperShadow(ref inHigh, ref inClose, ref inOpen, i - 3) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inOpen,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[3], i - 3) &&
                    // 2nd: marubozu
                    LowerShadow(ref inClose, ref inOpen, ref inLow, i - 2) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inOpen,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[2], i - 2) &&
                    UpperShadow(ref inHigh, ref inClose, ref inOpen, i - 2) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inOpen,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[2], i - 2) &&
                    RealBodyGapDown(ref inOpen, ref inClose, i - 1, i - 2) && // 3rd: opens gapping down
                    //      and HAS an upper shadow
                    UpperShadow(ref inHigh, ref inClose, ref inOpen, i - 1) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inOpen,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[1], i - 1) &&
                    inHigh[i - 1] > inClose[i - 2] && //      that extends into the prior body
                    inHigh[i] > inHigh[i - 1] && inLow[i] < inLow[i - 1] // 4th: engulfs the 3rd including the shadows
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
                for (var totIdx = 3; totIdx >= 1; --totIdx)
                {
                    shadowVeryShortPeriodTotal[totIdx] +=
                        CandleRange(ref inOpen, ref inHigh, ref inLow, ref inOpen, CandleSettingType.ShadowVeryShort, i - totIdx)
                        - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inOpen, CandleSettingType.ShadowVeryShort,
                            shadowVeryShortTrailingIdx - totIdx);
                }

                i++;
                shadowVeryShortTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }
        
        public static int ConcealBabysWallLookback() => CandleAvgPeriod(CandleSettingType.ShadowVeryShort) + 3;
    }
}
