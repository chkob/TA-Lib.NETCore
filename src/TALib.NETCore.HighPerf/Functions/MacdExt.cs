using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        // TODO: Find unit test data
        public static RetCode MacdExt(
            ref Span<decimal> input,
            ref Span<decimal> output,
            int inputSize,
            out int outputSize,
            MAType optInFastMAType = MAType.Sma,
            MAType optInSlowMAType = MAType.Sma,
            MAType optInSignalMAType = MAType.Sma,
            int optInFastPeriod = 12,
            int optInSlowPeriod = 26,
            int optInSignalPeriod = 9)
        {
            if (optInFastPeriod < 2 || optInFastPeriod > 100000 ||
                optInSlowPeriod < 2 || optInSlowPeriod > 100000 ||
                optInSignalPeriod < 1 || optInSignalPeriod > 100000)
            {
                outputSize = 0;
                return RetCode.BadParam;
            }

            var startIdx = 0;
            var endIdx = inputSize - 1;

            if (optInSlowPeriod < optInFastPeriod)
            {
                (optInSlowPeriod, optInFastPeriod) = (optInFastPeriod, optInSlowPeriod);
                (optInSlowMAType, optInFastMAType) = (optInFastMAType, optInSlowMAType);
            }

            var lookbackSignal = MaLookback(
                optInSignalMAType,
                optInSignalPeriod);

            var lookbackTotal = MacdExtLookback(
                optInFastMAType,
                optInSlowMAType,
                optInSignalMAType,
                optInFastPeriod,
                optInSlowPeriod,
                optInSignalPeriod);

            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                outputSize = 0;
                return RetCode.Success;
            }

            var tempInteger = endIdx - startIdx + 1 + lookbackSignal;
            var fastMABuffer = BufferHelpers.New(tempInteger);
            var slowMABuffer = BufferHelpers.New(tempInteger);

            tempInteger = startIdx - lookbackSignal;
            RetCode retCode = Ma(
                ref input,
                tempInteger,
                endIdx,
                ref slowMABuffer,
                out var outBegIdx1,
                out var outNbElement1,
                optInSlowMAType,
                optInSlowPeriod);

            if (retCode != RetCode.Success)
            {
                outputSize = 0;
                return retCode;
            }

            retCode = Ma(
                ref input,
                tempInteger,
                endIdx,
                ref fastMABuffer,
                out var outBegIdx2,
                out var outNbElement2,
                optInFastMAType,
                optInFastPeriod);

            if (retCode != RetCode.Success)
            {
                outputSize = 0;
                return retCode;
            }

            if (outBegIdx1 != tempInteger || outBegIdx2 != tempInteger || outNbElement1 != outNbElement2 ||
                outNbElement1 != endIdx - startIdx + 1 + lookbackSignal)
            {
                outputSize = 0;
                return RetCode.InternalError;
            }

            for (var i = 0; i < outNbElement1; i++)
            {
                fastMABuffer[i] -= slowMABuffer[i];
            }

            outputSize = endIdx - startIdx + 1;
            var outMacd = output.Series(outputSize, 0);
            var outMacdSignal = output.Series(outputSize, 1);
            var outMacdHist = output.Series(outputSize, 2);

            BufferHelpers.Copy(ref fastMABuffer, lookbackSignal, ref outMacd, 0, outputSize);

            retCode = Ma(
                ref fastMABuffer,
                0,
                outNbElement1 - 1,
                ref outMacdSignal,
                out _,
                out outNbElement2,
                optInSignalMAType,
                optInSignalPeriod);

            if (retCode != RetCode.Success)
            {
                outputSize = 0;
                return retCode;
            }

            for (var i = 0; i < outNbElement2; i++)
            {
                outMacdHist[i] = outMacd[i] - outMacdSignal[i];
            }

            output = output.Slice(0, 3 * outputSize);
            return RetCode.Success;
        }

        public static int MacdExtLookback(MAType optInFastMAType = MAType.Sma, MAType optInSlowMAType = MAType.Sma,
            MAType optInSignalMAType = MAType.Sma, int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
        {
            if (optInFastPeriod < 2 || optInFastPeriod > 100000 || optInSlowPeriod < 2 || optInSlowPeriod > 100000 ||
                optInSignalPeriod < 1 || optInSignalPeriod > 100000)
            {
                return -1;
            }

            int lookbackLargest = MaLookback(optInFastMAType, optInFastPeriod);
            int tempInteger = MaLookback(optInSlowMAType, optInSlowPeriod);
            if (tempInteger > lookbackLargest)
            {
                lookbackLargest = tempInteger;
            }

            return lookbackLargest + MaLookback(optInSignalMAType, optInSignalPeriod);
        }
    }
}
