using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode ThreeLineStrike(
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

            int lookbackTotal = ThreeLineStrikeLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            var nearPeriodTotal = BufferHelpers.New(4);
            int nearTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.Near);
            int i = nearTrailingIdx;
            while (i < startIdx)
            {
                nearPeriodTotal[3] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, i - 3);
                nearPeriodTotal[2] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, i - 2);
                i++;
            }

            i = startIdx;

            int outIdx = default;
            do
            {
                if (CandleColor(ref inClose, ref inOpen, i - 3) == CandleColor(ref inClose, ref inOpen, i - 2) &&
                    CandleColor(ref inClose, ref inOpen, i - 2) == CandleColor(ref inClose, ref inOpen, i - 1) &&
                    CandleColor(ref inClose, ref inOpen, i) == !CandleColor(ref inClose, ref inOpen, i - 1) &&
                    inOpen[i - 2] >= Math.Min(inOpen[i - 3], inClose[i - 3]) - CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.Near, nearPeriodTotal[3], i - 3) &&
                    inOpen[i - 2] <= Math.Max(inOpen[i - 3], inClose[i - 3]) + CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.Near, nearPeriodTotal[3], i - 3) &&
                    inOpen[i - 1] >= Math.Min(inOpen[i - 2], inClose[i - 2]) - CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.Near, nearPeriodTotal[2], i - 2) &&
                    inOpen[i - 1] <= Math.Max(inOpen[i - 2], inClose[i - 2]) + CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.Near, nearPeriodTotal[2], i - 2) &&
                    (
                        CandleColor(ref inClose, ref inOpen, i - 1) &&
                        inClose[i - 1] > inClose[i - 2] && inClose[i - 2] > inClose[i - 3] &&
                        inOpen[i] > inClose[i - 1] &&
                        inClose[i] < inOpen[i - 3]
                        ||
                        !CandleColor(ref inClose, ref inOpen, i - 1) &&
                        inClose[i - 1] < inClose[i - 2] && inClose[i - 2] < inClose[i - 3] &&
                        inOpen[i] < inClose[i - 1] &&
                        inClose[i] > inOpen[i - 3]
                    )
                )
                {
                    outInteger[outIdx++] = Convert.ToInt32(CandleColor(ref inClose, ref inOpen, i - 1)) * 100;
                }
                else
                {
                    outInteger[outIdx++] = 0;
                }

                for (var totIdx = 3; totIdx >= 2; --totIdx)
                {
                    nearPeriodTotal[totIdx] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, i - totIdx)
                                               - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near,
                                                   nearTrailingIdx - totIdx);
                }

                i++;
                nearTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int ThreeLineStrikeLookback() => CandleAvgPeriod(CandleSettingType.Near) + 3;
    }
}
