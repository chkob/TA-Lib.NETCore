using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Mama(
            ref Span<double> inReal,
            int startIdx,
            int endIdx,
            ref Span<double> outMama,
            ref Span<double> outFama,
            out int outBegIdx,
            out int outNbElement,
            double optInFastLimit = 0.5,
            double optInSlowLimit = 0.05)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inReal == null || outMama == null || outFama == null || optInFastLimit < 0.01 || optInFastLimit > 0.99 ||
                optInSlowLimit < 0.01 || optInSlowLimit > 0.99)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = MamaLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            const double rad2Deg = 180.0 / Math.PI;

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
            int i = 9;
            do
            {
                tempReal = inReal[today++];
                HighPerf.Lib.DoPriceWma(ref inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, tempReal, out _);
            } while (--i != 0);

            int hilbertIdx = default;
            var hilbertVariables = InitHilbertVariables<double>();

            int outIdx = default;

            double prevI2, prevQ2, re, im, mama, fama, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2, prevPhase;
            double period = prevI2 = prevQ2
                = re = im = mama = fama = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = prevPhase = default;
            while (today <= endIdx)
            {
                double tempReal2;
                double i2;
                double q2;

                double adjustedPrevPeriod = 0.075 * period + 0.54;

                double todayValue = inReal[today];
                HighPerf.Lib.DoPriceWma(ref inReal, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue,
                    todayValue, out var smoothedValue);
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

                    tempReal2 = !i1ForEvenPrev3.Equals(0.0) ? Math.Atan(hilbertVariables["q1"] / i1ForEvenPrev3) * rad2Deg : 0.0;
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
                    tempReal2 = !i1ForOddPrev3.Equals(0.0) ? Math.Atan(hilbertVariables["q1"] / i1ForOddPrev3) * rad2Deg : 0.0;
                }

                tempReal = prevPhase - tempReal2;
                prevPhase = tempReal2;
                if (tempReal < 1.0)
                {
                    tempReal = 1.0;
                }

                if (tempReal > 1.0)
                {
                    tempReal = optInFastLimit / tempReal;
                    if (tempReal < optInSlowLimit)
                    {
                        tempReal = optInSlowLimit;
                    }
                }
                else
                {
                    tempReal = optInFastLimit;
                }

                mama = tempReal * todayValue + (1.0 - tempReal) * mama;
                tempReal *= 0.5;
                fama = tempReal * mama + (1.0 - tempReal) * fama;
                if (today >= startIdx)
                {
                    outMama[outIdx] = mama;
                    outFama[outIdx++] = fama;
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

                tempReal2 = 1.5 * tempReal;
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
                today++;
            }

            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int MamaLookback() => (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.Mama] + 32;
    }
}
