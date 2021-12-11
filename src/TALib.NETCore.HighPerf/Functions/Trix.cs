using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Trix(
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

            if (inReal == null || outReal == null || optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int emaLookback = EmaLookback(optInTimePeriod);
            int lookbackTotal = TrixLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            outBegIdx = startIdx;
            int nbElementToOutput = endIdx - startIdx + 1 + lookbackTotal;
            var tempBuffer = BufferHelpers.New(nbElementToOutput);

            double k = 2.0 / (optInTimePeriod + 1);
            RetCode retCode = HighPerf.Lib.INT_EMA(ref inReal, startIdx - lookbackTotal, endIdx, ref tempBuffer, out _, out var nbElement, optInTimePeriod, k);
            if (retCode != RetCode.Success || nbElement == 0)
            {
                return retCode;
            }

            nbElementToOutput--;

            nbElementToOutput -= emaLookback;
            retCode = HighPerf.Lib.INT_EMA(ref tempBuffer, 0, nbElementToOutput, ref tempBuffer, out _, out nbElement, optInTimePeriod, k);
            if (retCode != RetCode.Success || nbElement == 0)
            {
                return retCode;
            }

            nbElementToOutput -= emaLookback;
            retCode = HighPerf.Lib.INT_EMA(ref tempBuffer, 0, nbElementToOutput, ref tempBuffer, out _, out nbElement, optInTimePeriod, k);
            if (retCode != RetCode.Success || nbElement == 0)
            {
                return retCode;
            }

            nbElementToOutput -= emaLookback;
            retCode = Roc(ref tempBuffer, 0, nbElementToOutput, ref outReal, out _, out outNbElement, 1);
            if (retCode != RetCode.Success || outNbElement == 0)
            {
                return retCode;
            }

            return RetCode.Success;
        }

        public static int TrixLookback(int optInTimePeriod = 30)
        {
            if (optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return EmaLookback(optInTimePeriod) * 3 + RocRLookback(1);
        }
    }
}
