using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode AdvanceBlock(
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

            int lookbackTotal = AdvanceBlockLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            var shadowShortPeriodTotal = BufferHelpers.New(3);
            var shadowLongPeriodTotal = BufferHelpers.New(2);
            var nearPeriodTotal = BufferHelpers.New(3);
            var farPeriodTotal = BufferHelpers.New(3);
            int shadowShortTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.ShadowShort);
            int shadowLongTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.ShadowLong);
            int nearTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.Near);
            int farTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.Far);
            decimal bodyLongPeriodTotal = default;
            int bodyLongTrailingIdx = startIdx - CandleAvgPeriod(CandleSettingType.BodyLong);
            int i = shadowShortTrailingIdx;
            while (i < startIdx)
            {
                shadowShortPeriodTotal[2] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowShort, i - 2);
                shadowShortPeriodTotal[1] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowShort, i - 1);
                shadowShortPeriodTotal[0] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowShort, i);
                i++;
            }

            i = shadowLongTrailingIdx;
            while (i < startIdx)
            {
                shadowLongPeriodTotal[1] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowLong, i - 1);
                shadowLongPeriodTotal[0] += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowLong, i);
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

            i = bodyLongTrailingIdx;
            while (i < startIdx)
            {
                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i - 2);
                i++;
            }

            i = startIdx;

            int outIdx = default;
            do
            {
                if (CandleColor(ref inClose, ref inOpen, i - 2) && // 1st white
                    CandleColor(ref inClose, ref inOpen, i - 1) && // 2nd white
                    CandleColor(ref inClose, ref inOpen, i) && // 3rd white
                    inClose[i] > inClose[i - 1] && inClose[i - 1] > inClose[i - 2] && // consecutive higher closes
                    inOpen[i - 1] > inOpen[i - 2] && // 2nd opens within/near 1st real body
                    inOpen[i - 1] <= inClose[i - 2] + CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near,
                        nearPeriodTotal[2], i - 2) &&
                    inOpen[i] > inOpen[i - 1] && // 3rd opens within/near 2nd real body
                    inOpen[i] <= inClose[i - 1] +
                    CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Near, nearPeriodTotal[1], i - 1) &&
                    RealBody(ref inClose, ref inOpen, i - 2) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong,
                        bodyLongPeriodTotal, i - 2) && // 1st: long real body
                    UpperShadow(ref inHigh, ref inClose, ref inOpen, i - 2) < CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                        CandleSettingType.ShadowShort, shadowShortPeriodTotal[2], i - 2) &&
                    // 1st: short upper shadow
                    // ( 2 far smaller than 1 && 3 not longer than 2 )
                    // advance blocked with the 2nd, 3rd must not carry on the advance
                    (RealBody(ref inClose, ref inOpen, i - 1) < RealBody(ref inClose, ref inOpen, i - 2) -
                     CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.Far, farPeriodTotal[2], i - 2) &&
                     RealBody(ref inClose, ref inOpen, i) < RealBody(ref inClose, ref inOpen, i - 1) + CandleAverage(ref inOpen, ref inHigh, ref inLow,
                         ref inClose, CandleSettingType.Near, nearPeriodTotal[1], i - 1)
                     ||
                     // 3 far smaller than 2
                     // advance blocked with the 3rd
                     RealBody(ref inClose, ref inOpen, i) < RealBody(ref inClose, ref inOpen, i - 1) - CandleAverage(ref inOpen, ref inHigh, ref inLow,
                         ref inClose, CandleSettingType.Far, farPeriodTotal[1], i - 1)
                     ||
                     // ( 3 smaller than 2 && 2 smaller than 1 && (3 or 2 not short upper shadow) )
                     // advance blocked with progressively smaller real bodies and some upper shadows
                     RealBody(ref inClose, ref inOpen, i) < RealBody(ref inClose, ref inOpen, i - 1) &&
                     RealBody(ref inClose, ref inOpen, i - 1) < RealBody(ref inClose, ref inOpen, i - 2) &&
                     (
                         UpperShadow(ref inHigh, ref inClose, ref inOpen, i) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                             CandleSettingType.ShadowShort, shadowShortPeriodTotal[0], i) ||
                         UpperShadow(ref inHigh, ref inClose, ref inOpen, i - 1) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                             CandleSettingType.ShadowShort, shadowShortPeriodTotal[1], i - 1)
                     ) ||
                     // ( 3 smaller than 2 && 3 long upper shadow )
                     // advance blocked with 3rd candle's long upper shadow and smaller body
                     RealBody(ref inClose, ref inOpen, i) < RealBody(ref inClose, ref inOpen, i - 1) &&
                     UpperShadow(ref inHigh, ref inClose, ref inOpen, i) > CandleAverage(ref inOpen, ref inHigh, ref inLow, ref inClose,
                         CandleSettingType.ShadowLong, shadowLongPeriodTotal[0], i)))
                {
                    outInteger[outIdx++] = -100;
                }
                else
                {
                    outInteger[outIdx++] = 0;
                }

                /* add the current range and subtract the first range: this is done after the pattern recognition
                 * when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)
                 */
                for (var totIdx = 2; totIdx >= 0; --totIdx)
                {
                    shadowShortPeriodTotal[totIdx] +=
                        CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowShort, i - totIdx)
                        - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowShort, shadowShortTrailingIdx - totIdx);
                }

                for (var totIdx = 1; totIdx >= 0; --totIdx)
                {
                    shadowLongPeriodTotal[totIdx] +=
                        CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowLong, i - totIdx)
                        - CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.ShadowLong, shadowLongTrailingIdx - totIdx);
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

                bodyLongPeriodTotal += CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, i - 2) -
                                       CandleRange(ref inOpen, ref inHigh, ref inLow, ref inClose, CandleSettingType.BodyLong, bodyLongTrailingIdx - 2);
                i++;
                shadowShortTrailingIdx++;
                shadowLongTrailingIdx++;
                nearTrailingIdx++;
                farTrailingIdx++;
                bodyLongTrailingIdx++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int AdvanceBlockLookback() =>
            Math.Max(
                Math.Max(
                    (sbyte) Math.Max(CandleAvgPeriod(CandleSettingType.ShadowLong), CandleAvgPeriod(CandleSettingType.ShadowShort)),
                    (sbyte) Math.Max(CandleAvgPeriod(CandleSettingType.Far), CandleAvgPeriod(CandleSettingType.Near))),
                CandleAvgPeriod(CandleSettingType.BodyLong)
            ) + 2;
    }
}
