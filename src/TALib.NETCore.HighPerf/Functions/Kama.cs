using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Kama(
            ref Span<decimal> input,
            ref Span<decimal> output,
            int inputSize,
            out int outputSize,
            int optInTimePeriod = 30)
        {
            var result = Kama(ref input, 0, inputSize - 1, ref output, out _, out outputSize, optInTimePeriod);
            output = output.Slice(0, outputSize);
            return result;
        }

        internal static RetCode Kama(
            ref Span<decimal> inReal,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
            out int outBegIdx,
            out int outNbElement,
            int optInTimePeriod = 30)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inReal == null || outReal == null || optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = KamaLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            const decimal constMax = 2.0m / (30.0m + 1.0m);
            const decimal constDiff = 2.0m / (2.0m + 1.0m) - constMax;

            decimal sumROC1 = default;
            decimal tempReal;
            int today = startIdx - lookbackTotal;
            int trailingIdx = today;
            int i = optInTimePeriod;
            while (i-- > 0)
            {
                tempReal = inReal[today++];
                tempReal -= inReal[today];
                sumROC1 += Math.Abs(tempReal);
            }

            decimal prevKAMA = inReal[today - 1];

            tempReal = inReal[today];
            decimal tempReal2 = inReal[trailingIdx++];
            decimal periodROC = tempReal - tempReal2;

            decimal trailingValue = tempReal2;
            if (sumROC1 <= periodROC || HighPerf.Lib.IsZero(sumROC1))
            {
                tempReal = 1.0m;
            }
            else
            {
                tempReal = Math.Abs(periodROC / sumROC1);
            }

            tempReal = tempReal * constDiff + constMax;
            tempReal *= tempReal;

            prevKAMA = (inReal[today++] - prevKAMA) * tempReal + prevKAMA;
            while (today <= startIdx)
            {
                tempReal = inReal[today];
                tempReal2 = inReal[trailingIdx++];
                periodROC = tempReal - tempReal2;

                sumROC1 -= Math.Abs(trailingValue - tempReal2);
                sumROC1 += Math.Abs(tempReal - inReal[today - 1]);

                trailingValue = tempReal2;
                if (sumROC1 <= periodROC || HighPerf.Lib.IsZero(sumROC1))
                {
                    tempReal = 1.0m;
                }
                else
                {
                    tempReal = Math.Abs(periodROC / sumROC1);
                }

                tempReal = tempReal * constDiff + constMax;
                tempReal *= tempReal;

                prevKAMA = (inReal[today++] - prevKAMA) * tempReal + prevKAMA;
            }

            outReal[0] = prevKAMA;
            int outIdx = 1;
            outBegIdx = today - 1;
            while (today <= endIdx)
            {
                tempReal = inReal[today];
                tempReal2 = inReal[trailingIdx++];
                periodROC = tempReal - tempReal2;

                sumROC1 -= Math.Abs(trailingValue - tempReal2);
                sumROC1 += Math.Abs(tempReal - inReal[today - 1]);

                trailingValue = tempReal2;
                if (sumROC1 <= periodROC || HighPerf.Lib.IsZero(sumROC1))
                {
                    tempReal = 1.0m;
                }
                else
                {
                    tempReal = Math.Abs(periodROC / sumROC1);
                }

                tempReal = tempReal * constDiff + constMax;
                tempReal *= tempReal;

                prevKAMA = (inReal[today++] - prevKAMA) * tempReal + prevKAMA;
                outReal[outIdx++] = prevKAMA;
            }

            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int KamaLookback(int optInTimePeriod = 30)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod + (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.Kama];
        }
    }
}
