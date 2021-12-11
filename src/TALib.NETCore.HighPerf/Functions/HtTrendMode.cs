using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode HtTrendMode(
            ref Span<decimal> inReal,
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

            if (inReal == null || outInteger == null)
            {
                return RetCode.BadParam;
            }

            const int smoothPriceSize = 50;
            var smoothPrice = BufferHelpers.New(smoothPriceSize);

            const decimal rad2Deg = 180.0m / (decimal)Math.PI;
            const decimal deg2Rad = 1.0m / rad2Deg;
            const decimal constDeg2RadBy360 = 2.0m * (decimal)Math.PI;

            decimal iTrend3 = default;
            decimal iTrend2 = iTrend3;
            decimal iTrend1 = iTrend2;
            int daysInTrend = default;
            decimal sine = default;
            decimal leadSine = default;

            int lookbackTotal = HtTrendModeLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

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

            decimal prevI2, prevQ2, re, im, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2, smoothPeriod, dcPhase;
            decimal period = prevI2 = prevQ2 =
                re = im = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = smoothPeriod = dcPhase = default;
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

                decimal prevDCPhase = dcPhase;
                decimal dcPeriod = smoothPeriod + 0.5m;
                var dcPeriodInt = (int) dcPeriod;
                decimal realPart = default;
                decimal imagPart = default;

                int idx = smoothPriceIdx;
                for (i = 0; i < dcPeriodInt; i++)
                {
                    tempReal = i * constDeg2RadBy360 / dcPeriodInt;
                    tempReal2 = smoothPrice[idx];
                    realPart += (decimal) Math.Sin((double)tempReal) * tempReal2;
                    imagPart += (decimal) Math.Cos((double)tempReal) * tempReal2;
                    if (idx == 0)
                    {
                        idx = smoothPriceSize - 1;
                    }
                    else
                    {
                        idx--;
                    }
                }

                tempReal = Math.Abs(imagPart);
                if (tempReal > 0.0m)
                {
                    dcPhase = (decimal) Math.Atan((double)(realPart / imagPart)) * rad2Deg;
                }
                else if (tempReal <= 0.01m)
                {
                    if (realPart < 0.0m)
                    {
                        dcPhase -= 90.0m;
                    }
                    else if (realPart > 0.0m)
                    {
                        dcPhase += 90.0m;
                    }
                }

                dcPhase += 90.0m;
                dcPhase += 360.0m / smoothPeriod;
                if (imagPart < 0.0m)
                {
                    dcPhase += 180.0m;
                }

                if (dcPhase > 315.0m)
                {
                    dcPhase -= 360.0m;
                }

                decimal prevSine = sine;
                decimal prevLeadSine = leadSine;
                sine = (decimal) Math.Sin((double)(dcPhase * deg2Rad));
                leadSine = (decimal) Math.Sin((double)(dcPhase + 45.0m)) * deg2Rad;

                dcPeriod = smoothPeriod + 0.5m;
                dcPeriodInt = (int) dcPeriod;

                idx = today;
                tempReal = default;
                for (i = 0; i < dcPeriodInt; i++)
                {
                    tempReal += inReal[idx--];
                }

                if (dcPeriodInt > 0)
                {
                    tempReal /= dcPeriodInt;
                }

                decimal trendline = (4.0m * tempReal + 3.0m * iTrend1 + 2.0m * iTrend2 + iTrend3) / 10.0m;
                iTrend3 = iTrend2;
                iTrend2 = iTrend1;
                iTrend1 = tempReal;

                int trend = 1;

                if (sine > leadSine && prevSine <= prevLeadSine || sine < leadSine && prevSine >= prevLeadSine)
                {
                    daysInTrend = 0;
                    trend = 0;
                }

                if (++daysInTrend < 0.5m * smoothPeriod)
                {
                    trend = 0;
                }

                tempReal = dcPhase - prevDCPhase;
                if (!smoothPeriod.Equals(0.0) && tempReal > 0.67m * 360.0m / smoothPeriod && tempReal < 1.5m * 360.0m / smoothPeriod)
                {
                    trend = 0;
                }

                tempReal = smoothPrice[smoothPriceIdx];
                if (!trendline.Equals(0.0m) && Math.Abs((tempReal - trendline) / trendline) >= 0.015m)
                {
                    trend = 1;
                }

                if (today >= startIdx)
                {
                    outInteger[outIdx++] = trend;
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

        public static int HtTrendModeLookback() => (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.HtTrendMode] + 63;
    }
}
