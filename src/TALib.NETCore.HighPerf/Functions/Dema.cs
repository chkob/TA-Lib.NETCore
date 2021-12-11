using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Dema(
            ref Span<double> input,
            ref Span<double> output,
            int inputSize,
            out int outputSize,
            int optInTimePeriod = 30)
        {
            var result = Dema(ref input, 0, inputSize - 1, ref output, out _, out outputSize, optInTimePeriod);
            output = output.Slice(0, outputSize);
            return result;
        }

        internal static RetCode Dema(
            ref Span<double> inReal,
            int startIdx,
            int endIdx,
            ref Span<double> outReal,
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

            int lookbackEMA = EmaLookback(optInTimePeriod);
            int lookbackTotal = DemaLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            Span<double> firstEMA;
            if (inReal == outReal)
            {
                firstEMA = outReal;
            }
            else
            {
                int tempInt = lookbackTotal + (endIdx - startIdx) + 1;
                firstEMA = BufferHelpers.New(tempInt);
            }

            double k = 2.0 / (optInTimePeriod + 1);
            RetCode retCode = INT_EMA(ref inReal, startIdx - lookbackEMA, endIdx, ref firstEMA, out var firstEMABegIdx,
                out var firstEMANbElement, optInTimePeriod, k);
            if (retCode != RetCode.Success || firstEMANbElement == 0)
            {
                return retCode;
            }

            var secondEMA = BufferHelpers.New(firstEMANbElement);

            retCode = INT_EMA(ref firstEMA, 0, firstEMANbElement - 1, ref secondEMA, out var secondEMABegIdx,
                out var secondEMANbElement, optInTimePeriod, k);
            if (retCode != RetCode.Success || secondEMANbElement == 0)
            {
                return retCode;
            }

            int firstEMAIdx = secondEMABegIdx;
            int outIdx = default;
            while (outIdx < secondEMANbElement)
            {
                outReal[outIdx] = 2.0 * firstEMA[firstEMAIdx++] - secondEMA[outIdx];
                outIdx++;
            }

            outBegIdx = firstEMABegIdx + secondEMABegIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int DemaLookback(int optInTimePeriod = 30)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return EmaLookback(optInTimePeriod) * 2;
        }
    }
}
