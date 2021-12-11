using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode HtTrendline(
            ref Span<decimal> inReal,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
            out int outBegIdx,
            out int outNbElement)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inReal == null || outReal == null)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = HtTrendlineLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            const int smoothPriceSize = 50;
            var smoothPrice = BufferHelpers.New(smoothPriceSize);

            decimal iTrend2, iTrend1;
            decimal iTrend3 = iTrend2 = iTrend1 = default;
            const decimal rad2Deg = 180.0m / (decimal)Math.PI;

            outBegIdx = startIdx;

            int trailingWMAIdx = startIdx - lookbackTotal;
            int today = trailingWMAIdx;

            decimal tempReal = inReal[today++];
            decimal periodWMASub = tempReal;
            decimal periodWMASum = tempReal;
            tempReal = inReal[today++];
            periodWMASub += tempReal;
            periodWMASum += tempReal * 2.0m;
            tempReal = inReal[today++];
            periodWMASub += tempReal;
            periodWMASum += tempReal * 3.0m;

            decimal trailingWMAValue = default;

            var i = 34;
            do
            {
                tempReal = inReal[today++];
                HighPerf.Lib.DoPriceWma(ref inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, tempReal, out _);
            } while (--i != 0);

            int hilbertIdx = default;
            int smoothPriceIdx = default;

            var hilbertVariables = InitHilbertVariables<decimal>();

            int outIdx = default;

            decimal prevI2, prevQ2, re, im, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2, smoothPeriod;
            decimal period = prevI2 = prevQ2 =
                re = im = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = smoothPeriod = default;
            while (today <= endIdx)
            {
                decimal i2;
                decimal q2;

                decimal adjustedPrevPeriod = 0.075m * period + 0.54m;

                HighPerf.Lib.DoPriceWma(ref inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, inReal[today],
                    out var smoothedValue);

                smoothPrice[smoothPriceIdx] = smoothedValue;
                if (today % 2 == 0)
                {
                    HighPerf.Lib.DoHilbertEven(hilbertVariables, "detrender", smoothedValue, hilbertIdx, adjustedPrevPeriod);
                    HighPerf.Lib.DoHilbertEven(hilbertVariables, "q1", hilbertVariables["detrender"], hilbertIdx, adjustedPrevPeriod);
                    HighPerf.Lib.DoHilbertEven(hilbertVariables, "jI", i1ForEvenPrev3, hilbertIdx, adjustedPrevPeriod);
                    HighPerf.Lib.DoHilbertEven(hilbertVariables, "jQ", hilbertVariables["q1"], hilbertIdx, adjustedPrevPeriod);

                    if (++hilbertIdx == 3)
                    {
                        hilbertIdx = 0;
                    }

                    q2 = 0.2m * (hilbertVariables["q1"] + hilbertVariables["jI"]) + 0.8m * prevQ2;
                    i2 = 0.2m * (i1ForEvenPrev3 - hilbertVariables["jQ"]) + 0.8m * prevI2;

                    i1ForOddPrev3 = i1ForOddPrev2;
                    i1ForOddPrev2 = hilbertVariables["detrender"];
                }
                else
                {
                    HighPerf.Lib.DoHilbertOdd(hilbertVariables, "detrender", smoothedValue, hilbertIdx, adjustedPrevPeriod);
                    HighPerf.Lib.DoHilbertOdd(hilbertVariables, "q1", hilbertVariables["detrender"], hilbertIdx, adjustedPrevPeriod);
                    HighPerf.Lib.DoHilbertOdd(hilbertVariables, "jI", i1ForOddPrev3, hilbertIdx, adjustedPrevPeriod);
                    HighPerf.Lib.DoHilbertOdd(hilbertVariables, "jQ", hilbertVariables["q1"], hilbertIdx, adjustedPrevPeriod);

                    q2 = 0.2m * (hilbertVariables["q1"] + hilbertVariables["jI"]) + 0.8m * prevQ2;
                    i2 = 0.2m * (i1ForOddPrev3 - hilbertVariables["jQ"]) + 0.8m * prevI2;

                    i1ForEvenPrev3 = i1ForEvenPrev2;
                    i1ForEvenPrev2 = hilbertVariables["detrender"];
                }

                re = 0.2m * (i2 * prevI2 + q2 * prevQ2) + 0.8m * re;
                im = 0.2m * (i2 * prevQ2 - q2 * prevI2) + 0.8m * im;
                prevQ2 = q2;
                prevI2 = i2;
                tempReal = period;
                if (!im.Equals(0.0) && !re.Equals(0.0))
                {
                    period = 360.0m / ((decimal) Math.Atan((double)(im / re)) * rad2Deg);
                }

                decimal tempReal2 = 1.5m * tempReal;
                if (period > tempReal2)
                {
                    period = tempReal2;
                }

                tempReal2 = 0.67m * tempReal;
                if (period < tempReal2)
                {
                    period = tempReal2;
                }

                if (period < 6.0m)
                {
                    period = 6.0m;
                }
                else if (period > 50.0m)
                {
                    period = 50.0m;
                }

                period = 0.2m * period + 0.8m * tempReal;

                smoothPeriod = 0.33m * period + 0.67m * smoothPeriod;

                decimal dcPeriod = smoothPeriod + 0.5m;
                var dcPeriodInt = (int) dcPeriod;

                int idx = today;
                tempReal = default;
                for (i = 0; i < dcPeriodInt; i++)
                {
                    tempReal += inReal[idx--];
                }

                if (dcPeriodInt > 0)
                {
                    tempReal /= dcPeriodInt;
                }

                tempReal2 = (4.0m * tempReal + 3.0m * iTrend1 + 2.0m * iTrend2 + iTrend3) / 10.0m;
                iTrend3 = iTrend2;
                iTrend2 = iTrend1;
                iTrend1 = tempReal;

                if (today >= startIdx)
                {
                    outReal[outIdx++] = tempReal2;
                }

                if (++smoothPriceIdx > smoothPriceSize - 1)
                {
                    smoothPriceIdx = 0;
                }

                today++;
            }

            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int HtTrendlineLookback() => (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.HtTrendline] + 63;
    }
}
