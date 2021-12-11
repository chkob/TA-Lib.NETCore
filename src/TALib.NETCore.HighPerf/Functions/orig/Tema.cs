using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Tema(
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
            int lookbackTotal = TemaLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            int tempInt = lookbackTotal + (endIdx - startIdx) + 1;
            double k = 2.0 / (optInTimePeriod + 1);

            var firstEMA = BufferHelpers.New(tempInt);
            RetCode retCode = HighPerf.Lib.INT_EMA(ref inReal, startIdx - lookbackEMA * 2, endIdx, ref firstEMA, out var firstEMABegIdx,
                out var firstEMANbElement, optInTimePeriod, k);
            if (retCode != RetCode.Success || firstEMANbElement == 0)
            {
                return retCode;
            }

            var secondEMA = BufferHelpers.New(firstEMANbElement);
            retCode = HighPerf.Lib.INT_EMA(ref firstEMA, 0, firstEMANbElement - 1, ref secondEMA, out var secondEMABegIdx, out var secondEMANbElement,
                optInTimePeriod, k);
            if (retCode != RetCode.Success || secondEMANbElement == 0)
            {
                return retCode;
            }

            retCode = HighPerf.Lib.INT_EMA(ref secondEMA, 0, secondEMANbElement - 1, ref outReal, out var thirdEMABegIdx, out var thirdEMANbElement,
                optInTimePeriod, k);
            if (retCode != RetCode.Success || thirdEMANbElement == 0)
            {
                return retCode;
            }

            int firstEMAIdx = thirdEMABegIdx + secondEMABegIdx;
            int secondEMAIdx = thirdEMABegIdx;
            outBegIdx = firstEMAIdx + firstEMABegIdx;
            int outIdx = default;
            while (outIdx < thirdEMANbElement)
            {
                outReal[outIdx++] += 3.0 * firstEMA[firstEMAIdx++] - 3.0 * secondEMA[secondEMAIdx++];
            }

            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int TemaLookback(int optInTimePeriod = 30)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return EmaLookback(optInTimePeriod) * 3;
        }
    }
}
