using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode ThreeInside(
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

            int lookbackTotal = ThreeInsideLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            double bodyLongPeriodTotal = default;
            double bodyShortPeriodTotal = default;
            int bodyLongTrailingIdx = startIdx - 2 - CandleAvgPeriod(CandleSettingType.BodyLong);
            int bodyShortTrailingIdx = startIdx - 1 - CandleAvgPeriod(CandleSettingType.BodyShort);

            int i = bodyLongTrailingIdx;
            while (i < startIdx - 2)
            {
                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i);
                i++;
            }

            i = bodyShortTrailingIdx;
            while (i < startIdx - 1)
            {
                bodyShortPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i);
                i++;
            }

            i = startIdx;

            int outIdx = default;
            do
            {
                if (RealBody(ref inClose, ref inOpen, i - 2) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong,
                        bodyLongPeriodTotal, i - 2) &&
                    RealBody(ref inClose, ref inOpen, i - 1) <= CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort,
                        bodyShortPeriodTotal, i - 1) &&
                    Math.Max(inClose[i - 1], inOpen[i - 1]) < Math.Max(inClose[i - 2], inOpen[i - 2]) &&
                    Math.Min(inClose[i - 1], inOpen[i - 1]) > Math.Min(inClose[i - 2], inOpen[i - 2]) &&
                    (CandleColor(ref inClose, ref inOpen, i - 2) && !CandleColor(ref inClose, ref inOpen, i) && inClose[i] < inOpen[i - 2]
                     ||
                     !CandleColor(ref inClose, ref inOpen, i - 2) && CandleColor(ref inClose, ref inOpen, i) && inClose[i] > inOpen[i - 2]
                    ))
                {
                    outInteger[outIdx++] = Convert.ToInt32(!CandleColor(ref inClose, ref inOpen, i - 2)) * 100;
                }
                else
                {
                    outInteger[outIdx++] = 0;
                }

                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i - 2) -
                                       CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, bodyLongTrailingIdx);
                bodyShortPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, i - 1) -
                                        CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyShort, bodyShortTrailingIdx);
                i++;
                bodyLongTrailingIdx++;
                bodyShortTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int ThreeInsideLookback() =>
            Math.Max(CandleAvgPeriod(CandleSettingType.BodyShort), CandleAvgPeriod(CandleSettingType.BodyLong)) + 2;
    }
}
