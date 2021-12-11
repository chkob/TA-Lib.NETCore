using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode MacdExt(ref Span<double> inReal, int startIdx, int endIdx, ref Span<double> outMacd, ref Span<double> outMacdSignal,
            ref Span<double> outMacdHist, out int outBegIdx, out int outNbElement, MAType optInFastMAType = MAType.Sma,
            MAType optInSlowMAType = MAType.Sma, MAType optInSignalMAType = MAType.Sma, int optInFastPeriod = 12, int optInSlowPeriod = 26,
            int optInSignalPeriod = 9)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inReal == null || outMacd == null || outMacdSignal == null || outMacdHist == null || optInFastPeriod < 2 ||
                optInFastPeriod > 100000 || optInSlowPeriod < 2 || optInSlowPeriod > 100000 || optInSignalPeriod < 1 ||
                optInSignalPeriod > 100000)
            {
                return RetCode.BadParam;
            }

            if (optInSlowPeriod < optInFastPeriod)
            {
                (optInSlowPeriod, optInFastPeriod) = (optInFastPeriod, optInSlowPeriod);
                (optInSlowMAType, optInFastMAType) = (optInFastMAType, optInSlowMAType);
            }

            int lookbackSignal = MaLookback(optInSignalMAType, optInSignalPeriod);
            int lookbackTotal = MacdExtLookback(optInFastMAType, optInSlowMAType, optInSignalMAType, optInFastPeriod, optInSlowPeriod,
                optInSignalPeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            var tempInteger = endIdx - startIdx + 1 + lookbackSignal;
            var fastMABuffer = BufferHelpers.New(tempInteger);
            var slowMABuffer = BufferHelpers.New(tempInteger);

            tempInteger = startIdx - lookbackSignal;
            RetCode retCode = Ma(ref inReal, tempInteger, endIdx, ref slowMABuffer, out var outBegIdx1, out var outNbElement1, optInSlowMAType,
                optInSlowPeriod);
            if (retCode != RetCode.Success)
            {
                return retCode;
            }

            retCode = Ma(ref inReal, tempInteger, endIdx, ref fastMABuffer, out var outBegIdx2, out var outNbElement2, optInFastMAType,
                optInFastPeriod);
            if (retCode != RetCode.Success)
            {
                return retCode;
            }

            if (outBegIdx1 != tempInteger || outBegIdx2 != tempInteger || outNbElement1 != outNbElement2 ||
                outNbElement1 != endIdx - startIdx + 1 + lookbackSignal)
            {
                return RetCode.InternalError;
            }

            for (var i = 0; i < outNbElement1; i++)
            {
                fastMABuffer[i] -= slowMABuffer[i];
            }

            BufferHelpers.Copy(ref fastMABuffer, lookbackSignal, ref outMacd, 0, endIdx - startIdx + 1);
            retCode = Ma(ref fastMABuffer, 0, outNbElement1 - 1, ref outMacdSignal, out _, out outNbElement2, optInSignalMAType, optInSignalPeriod);
            if (retCode != RetCode.Success)
            {
                return retCode;
            }

            for (var i = 0; i < outNbElement2; i++)
            {
                outMacdHist[i] = outMacd[i] - outMacdSignal[i];
            }

            outBegIdx = startIdx;
            outNbElement = outNbElement2;

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
