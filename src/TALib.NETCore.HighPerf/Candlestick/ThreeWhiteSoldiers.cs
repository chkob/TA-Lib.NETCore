using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode ThreeWhiteSoldiers(
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

            int lookbackTotal = ThreeWhiteSoldiersLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            var shadowVeryShortPeriodTotal = BufferHelpers.New(3);
            var nearPeriodTotal = BufferHelpers.New(3);
            var farPeriodTotal = BufferHelpers.New(3);
            int shadowVeryShortTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.ShadowVeryShort);
            int nearTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.Near);
            int farTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.Far);
            double bodyShortPeriodTotal = default;
            int bodyShortTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyShort);

            int i = shadowVeryShortTrailingIdx;
            while (i < startIdx)
            {
                shadowVeryShortPeriodTotal[2] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i - 2);
                shadowVeryShortPeriodTotal[1] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i - 1);
                shadowVeryShortPeriodTotal[0] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowVeryShort, i);
                i++;
            }

            i = nearTrailingIdx;
            while (i < startIdx)
            {
                nearPeriodTotal[2] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, i - 2);
                nearPeriodTotal[1] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, i - 1);
                i++;
            }

            i = farTrailingIdx;
            while (i < startIdx)
            {
                farPeriodTotal[2] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Far, i - 2);
                farPeriodTotal[1] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Far, i - 1);
                i++;
            }

            i = bodyShortTrailingIdx;
            while (i < startIdx)
            {
                bodyShortPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i);
                i++;
            }

            i = startIdx;

            int outIdx = default;
            do
            {
                if (CandleColor(ref inClose, ref inOpen, i - 2) &&
                    UpperShadow(ref inHigh, ref inClose, ref inOpen, i - 2) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[2], i - 2) &&
                    CandleColor(ref inClose, ref inOpen, i - 1) &&
                    UpperShadow(ref inHigh, ref inClose, ref inOpen, i - 1) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[1], i - 1) &&
                    CandleColor(ref inClose, ref inOpen, i) &&
                    UpperShadow(ref inHigh, ref inClose, ref inOpen, i) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowVeryShort, shadowVeryShortPeriodTotal[0], i) &&
                    inClose[i] > inClose[i - 1] && inClose[i - 1] > inClose[i - 2] &&
                    inOpen[i - 1] > inOpen[i - 2] &&
                    inOpen[i - 1] <= inClose[i - 2] + CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near,
                        nearPeriodTotal[2], i - 2) &&
                    inOpen[i] > inOpen[i - 1] &&
                    inOpen[i] <= inClose[i - 1] +
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, nearPeriodTotal[1], i - 1) &&
                    RealBody(ref inClose, ref inOpen, i - 1) > RealBody(ref inClose, ref inOpen, i - 2) - CandleAverage(ref inOpen, ref inHigh,
                        ref inLow, ref inClose, CandleSettingType.Far, farPeriodTotal[2], i - 2) &&
                    RealBody(ref inClose, ref inOpen, i) > RealBody(ref inClose, ref inOpen, i - 1) - CandleAverage(ref inOpen, ref inHigh, ref inLow,
                        ref inClose, CandleSettingType.Far, farPeriodTotal[1], i - 1) &&
                    RealBody(ref inClose, ref inOpen, i) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort,
                        bodyShortPeriodTotal, i))
                {
                    outInteger[outIdx++] = 100;
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

                for (var totIdx = 2; totIdx >= 1; --totIdx)
                {
                    farPeriodTotal[totIdx] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Far, i - totIdx)
                                              - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Far,
                                                  farTrailingIdx - totIdx);
                    nearPeriodTotal[totIdx] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, i - totIdx)
                                               - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near,
                                                   nearTrailingIdx - totIdx);
                }

                bodyShortPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i) -
                                        CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, bodyShortTrailingIdx);
                i++;
                shadowVeryShortTrailingIdx++;
                nearTrailingIdx++;
                farTrailingIdx++;
                bodyShortTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int ThreeWhiteSoldiersLookback() =>
            Math.Max(
                (sbyte) Math.Max(CandleAvgPeriod(CandleSettingType.ShadowVeryShort), CandleAvgPeriod(CandleSettingType.BodyShort)),
                (sbyte) Math.Max(CandleAvgPeriod(CandleSettingType.Far), CandleAvgPeriod(CandleSettingType.Near))
            ) + 2;
    }
}
