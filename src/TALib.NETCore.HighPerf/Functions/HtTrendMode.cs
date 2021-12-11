using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode HtTrendMode(
            ref Span<double> inReal,
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

            const double rad2Deg = 180.0 / Math.PI;
            const double deg2Rad = 1.0 / rad2Deg;
            const double constDeg2RadBy360 = 2.0 * Math.PI;

            double iTrend3 = default;
            double iTrend2 = iTrend3;
            double iTrend1 = iTrend2;
            int daysInTrend = default;
            double sine = default;
            double leadSine = default;

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
            double tempReal = inReal[today++];
            double periodWMASub = tempReal;
            double periodWMASum = tempReal;
            tempReal = inReal[today++];
            periodWMASub += tempReal;
            periodWMASum += tempReal * 2.0;
            tempReal = inReal[today++];
            periodWMASub += tempReal;
            periodWMASum += tempReal * 3.0;
            double trailingWMAValue = default;
            var i = 34;
            do
            {
                tempReal = inReal[today++];
                HighPerf.Lib.DoPriceWma(ref inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, tempReal, out _);
            } while (--i != 0);

            int hilbertIdx = default;
            int smoothPriceIdx = default;

            var hilbertVariables = InitHilbertVariables<double>();

            int outIdx = default;

            double prevI2, prevQ2, re, im, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2, smoothPeriod, dcPhase;
            double period = prevI2 = prevQ2 =
                re = im = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = smoothPeriod = dcPhase = default;
            while (today <= endIdx)
            {
                double i2;
                double q2;

                double adjustedPrevPeriod = 0.075 * period + 0.54;

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

                    q2 = 0.2 * (hilbertVariables["q1"] + hilbertVariables["jI"]) + 0.8 * prevQ2;
                    i2 = 0.2 * (i1ForEvenPrev3 - hilbertVariables["jQ"]) + 0.8 * prevI2;

                    i1ForOddPrev3 = i1ForOddPrev2;
                    i1ForOddPrev2 = hilbertVariables["detrender"];
                }
                else
                {
                    HighPerf.Lib.DoHilbertOdd(hilbertVariables, "detrender", smoothedValue, hilbertIdx, adjustedPrevPeriod);
                    HighPerf.Lib.DoHilbertOdd(hilbertVariables, "q1", hilbertVariables["detrender"], hilbertIdx, adjustedPrevPeriod);
                    HighPerf.Lib.DoHilbertOdd(hilbertVariables, "jI", i1ForOddPrev3, hilbertIdx, adjustedPrevPeriod);
                    HighPerf.Lib.DoHilbertOdd(hilbertVariables, "jQ", hilbertVariables["q1"], hilbertIdx, adjustedPrevPeriod);

                    q2 = 0.2 * (hilbertVariables["q1"] + hilbertVariables["jI"]) + 0.8 * prevQ2;
                    i2 = 0.2 * (i1ForOddPrev3 - hilbertVariables["jQ"]) + 0.8 * prevI2;

                    i1ForEvenPrev3 = i1ForEvenPrev2;
                    i1ForEvenPrev2 = hilbertVariables["detrender"];
                }

                re = 0.2 * (i2 * prevI2 + q2 * prevQ2) + 0.8 * re;
                im = 0.2 * (i2 * prevQ2 - q2 * prevI2) + 0.8 * im;
                prevQ2 = q2;
                prevI2 = i2;
                tempReal = period;
                if (!im.Equals(0.0) && !re.Equals(0.0))
                {
                    period = 360.0 / (Math.Atan(im / re) * rad2Deg);
                }

                double tempReal2 = 1.5 * tempReal;
                if (period > tempReal2)
                {
                    period = tempReal2;
                }

                tempReal2 = 0.67 * tempReal;
                if (period < tempReal2)
                {
                    period = tempReal2;
                }

                if (period < 6.0)
                {
                    period = 6.0;
                }
                else if (period > 50.0)
                {
                    period = 50.0;
                }

                period = 0.2 * period + 0.8 * tempReal;

                smoothPeriod = 0.33 * period + 0.67 * smoothPeriod;

                double prevDCPhase = dcPhase;
                double dcPeriod = smoothPeriod + 0.5;
                var dcPeriodInt = (int) dcPeriod;
                double realPart = default;
                double imagPart = default;

                int idx = smoothPriceIdx;
                for (i = 0; i < dcPeriodInt; i++)
                {
                    tempReal = i * constDeg2RadBy360 / dcPeriodInt;
                    tempReal2 = smoothPrice[idx];
                    realPart += Math.Sin(tempReal) * tempReal2;
                    imagPart += Math.Cos(tempReal) * tempReal2;
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
                if (tempReal > 0.0)
                {
                    dcPhase = Math.Atan(realPart / imagPart) * rad2Deg;
                }
                else if (tempReal <= 0.01)
                {
                    if (realPart < 0.0)
                    {
                        dcPhase -= 90.0;
                    }
                    else if (realPart > 0.0)
                    {
                        dcPhase += 90.0;
                    }
                }

                dcPhase += 90.0;
                dcPhase += 360.0 / smoothPeriod;
                if (imagPart < 0.0)
                {
                    dcPhase += 180.0;
                }

                if (dcPhase > 315.0)
                {
                    dcPhase -= 360.0;
                }

                double prevSine = sine;
                double prevLeadSine = leadSine;
                sine = Math.Sin(dcPhase * deg2Rad);
                leadSine = Math.Sin((dcPhase + 45.0) * deg2Rad);

                dcPeriod = smoothPeriod + 0.5;
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

                double trendline = (4.0 * tempReal + 3.0 * iTrend1 + 2.0 * iTrend2 + iTrend3) / 10.0;
                iTrend3 = iTrend2;
                iTrend2 = iTrend1;
                iTrend1 = tempReal;

                int trend = 1;

                if (sine > leadSine && prevSine <= prevLeadSine || sine < leadSine && prevSine >= prevLeadSine)
                {
                    daysInTrend = 0;
                    trend = 0;
                }

                if (++daysInTrend < 0.5 * smoothPeriod)
                {
                    trend = 0;
                }

                tempReal = dcPhase - prevDCPhase;
                if (!smoothPeriod.Equals(0.0) && tempReal > 0.67 * 360.0 / smoothPeriod && tempReal < 1.5 * 360.0 / smoothPeriod)
                {
                    trend = 0;
                }

                tempReal = smoothPrice[smoothPriceIdx];
                if (!trendline.Equals(0.0) && Math.Abs((tempReal - trendline) / trendline) >= 0.015)
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
