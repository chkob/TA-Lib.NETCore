using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode LadderBottom(
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

            int lookbackTotal = LadderBottomLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            double shadowVeryShortPeriodTotal = default;
            int shadowVeryShortTrailingIdx = startIdx - HighPerf.Lib.Globals.CandleSettings[(int) CandleSettingType.ShadowVeryShort].AvgPeriod;
            int i = shadowVeryShortTrailingIdx;
            while (i < startIdx)
            {
                shadowVeryShortPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i - 1);
                i++;
            }

            i = startIdx;
            int outIdx = default;
            do
            {
                if (!CandleColor(ref inClose, ref inOpen, i - 4) && !CandleColor(ref inClose, ref inOpen, i - 3) &&
                    !CandleColor(ref inClose, ref inOpen, i - 2) && // 3 black candlesticks
                    inOpen[i - 4] > inOpen[i - 3] && inOpen[i - 3] > inOpen[i - 2] && // with consecutively lower opens
                    inClose[i - 4] > inClose[i - 3] && inClose[i - 3] > inClose[i - 2] && // and closes
                    CandleColor(ref inClose, ref inOpen, i - 1) && // 4th: black with an upper shadow
                    UpperShadow(ref inHigh, ref inClose, ref inOpen, i - 1) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal, i - 1) &&
                    CandleColor(ref inClose, ref inOpen, i) && // 5th: white
                    inOpen[i] > inOpen[i - 1] && // that opens above prior candle's body
                    inClose[i] > inHigh[i - 1]) // and closes above prior candle's high
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
                shadowVeryShortPeriodTotal +=
                    CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i - 1)
                    - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, shadowVeryShortTrailingIdx - 1);
                i++;
                shadowVeryShortTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int LadderBottomLookback() => CandleAvgPeriod(CandleSettingType.ShadowVeryShort) + 4;
    }
}
