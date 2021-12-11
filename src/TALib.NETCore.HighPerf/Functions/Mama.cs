using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        // TODO: Find unit test data
        public static RetCode Mama(
            ref Span<decimal> input,
            ref Span<decimal> output,
            int inputSize,
            out int outputSize,
            decimal optInFastLimit = 0.5m,
            decimal optInSlowLimit = 0.05m)
        {
            if (optInFastLimit < 0.01m || optInFastLimit > 0.99m ||
                optInSlowLimit < 0.01m || optInSlowLimit > 0.99m)
            {
                outputSize = 0;
                return RetCode.BadParam;
            }

            var startIdx = 0;
            var endIdx = inputSize - 1;

            var lookbackTotal = MamaLookback();
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                outputSize = 0;
                return RetCode.Success;
            }

            const decimal rad2Deg = 180.0m / (decimal)(Math.PI);

           // outBegIdx = startIdx;

            int trailingWMAIdx = startIdx - lookbackTotal;
            int today = trailingWMAIdx;

            decimal tempReal = input[today++];
            decimal periodWMASub = tempReal;
            decimal periodWMASum = tempReal;
            tempReal = input[today++];
            periodWMASub += tempReal;
            periodWMASum += tempReal * 2.0m;
            tempReal = input[today++];
            periodWMASub += tempReal;
            periodWMASum += tempReal * 3.0m;

            var trailingWMAValue = 0.0m;
            var i = 9;
            do
            {
                tempReal = input[today++];
                DoPriceWma(ref input, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue, tempReal, out _);
            } while (--i != 0);

            int hilbertIdx = default;
            var hilbertVariables = InitHilbertVariables<decimal>();

            int outIdx = default;

            decimal prevI2, prevQ2, re, im, mama, fama, i1ForOddPrev3, i1ForEvenPrev3, i1ForOddPrev2, i1ForEvenPrev2, prevPhase;
            decimal period = prevI2 = prevQ2
                = re = im = mama = fama = i1ForOddPrev3 = i1ForEvenPrev3 = i1ForOddPrev2 = i1ForEvenPrev2 = prevPhase = default;

            outputSize = endIdx - today + 1;
            var outMama = output.Series(outputSize, 0);
            var outFama = output.Series(outputSize, 1);

            while (today <= endIdx)
            {
                decimal tempReal2;
                decimal i2;
                decimal q2;

                decimal adjustedPrevPeriod = 0.075m * period + 0.54m;

                decimal todayValue = input[today];
                DoPriceWma(ref input, ref trailingWMAIdx, ref periodWMASub, ref periodWMASum, ref trailingWMAValue,
                    todayValue, out var smoothedValue);
                if (today % 2 == 0)
                {
                    DoHilbertEven(hilbertVariables, "detrender", smoothedValue, hilbertIdx, adjustedPrevPeriod);
                    DoHilbertEven(hilbertVariables, "q1", hilbertVariables["detrender"], hilbertIdx, adjustedPrevPeriod);
                    DoHilbertEven(hilbertVariables, "jI", i1ForEvenPrev3, hilbertIdx, adjustedPrevPeriod);
                    DoHilbertEven(hilbertVariables, "jQ", hilbertVariables["q1"], hilbertIdx, adjustedPrevPeriod);

                    if (++hilbertIdx == 3)
                    {
                        hilbertIdx = 0;
                    }

                    q2 = 0.2m * (hilbertVariables["q1"] + hilbertVariables["jI"]) + 0.8m * prevQ2;
                    i2 = 0.2m * (i1ForEvenPrev3 - hilbertVariables["jQ"]) + 0.8m * prevI2;

                    i1ForOddPrev3 = i1ForOddPrev2;
                    i1ForOddPrev2 = hilbertVariables["detrender"];

                    tempReal2 = !i1ForEvenPrev3.Equals(0.0m) ? ((decimal) Math.Atan((double)hilbertVariables["q1"]) / i1ForEvenPrev3) * rad2Deg : 0.0m;
                }
                else
                {
                    DoHilbertOdd(hilbertVariables, "detrender", smoothedValue, hilbertIdx, adjustedPrevPeriod);
                    DoHilbertOdd(hilbertVariables, "q1", hilbertVariables["detrender"], hilbertIdx, adjustedPrevPeriod);
                    DoHilbertOdd(hilbertVariables, "jI", i1ForOddPrev3, hilbertIdx, adjustedPrevPeriod);
                    DoHilbertOdd(hilbertVariables, "jQ", hilbertVariables["q1"], hilbertIdx, adjustedPrevPeriod);

                    q2 = 0.2m * (hilbertVariables["q1"] + hilbertVariables["jI"]) + 0.8m * prevQ2;
                    i2 = 0.2m * (i1ForOddPrev3 - hilbertVariables["jQ"]) + 0.8m * prevI2;

                    i1ForEvenPrev3 = i1ForEvenPrev2;
                    i1ForEvenPrev2 = hilbertVariables["detrender"];
                    tempReal2 = !i1ForOddPrev3.Equals(0.0m) ? ((decimal) Math.Atan((double)hilbertVariables["q1"]) / i1ForOddPrev3) * rad2Deg : 0.0m;
                }

                tempReal = prevPhase - tempReal2;
                prevPhase = tempReal2;
                if (tempReal < 1.0m)
                {
                    tempReal = 1.0m;
                }

                if (tempReal > 1.0m)
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

                mama = tempReal * todayValue + (1.0m - tempReal) * mama;
                tempReal *= 0.5m;
                fama = tempReal * mama + (1.0m - tempReal) * fama;
                if (today >= startIdx)
                {
                    outMama[outIdx] = mama;
                    outFama[outIdx++] = fama;
                }

                re = 0.2m * (i2 * prevI2 + q2 * prevQ2) + 0.8m * re;
                im = 0.2m * (i2 * prevQ2 - q2 * prevI2) + 0.8m * im;
                prevQ2 = q2;
                prevI2 = i2;
                tempReal = period;
                if (!im.Equals(0.0m) && !re.Equals(0.0m))
                {
                    period = 360.0m / ((decimal)Math.Atan((double)(im / re)) * rad2Deg);
                }

                tempReal2 = 1.5m * tempReal;
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
                today++;
            }

            output = output.Slice(0, 2 * outputSize);
            return RetCode.Success;
        }

        public static int MamaLookback() => (int) Globals.UnstablePeriod[(int) FuncUnstId.Mama] + 32;
    }
}
