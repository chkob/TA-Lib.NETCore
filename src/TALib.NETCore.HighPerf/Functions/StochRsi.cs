using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode StochRsi(
            ref Span<decimal> inReal,
            int startIdx,
            int endIdx,
            ref Span<decimal> outFastK,
            ref Span<decimal> outFastD,
            out int outBegIdx,
            out int outNbElement,
            MAType optInFastDMAType = MAType.Sma,
            int optInTimePeriod = 14,
            int optInFastKPeriod = 5,
            int optInFastDPeriod = 3)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inReal == null || outFastK == null || outFastD == null || optInTimePeriod < 2 || optInTimePeriod > 100000 ||
                optInFastKPeriod < 1 || optInFastKPeriod > 100000 || optInFastDPeriod < 1 || optInFastDPeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackSTOCHF = StochFLookback(optInFastDMAType, optInFastKPeriod, optInFastDPeriod);
            int lookbackTotal = StochRsiLookback(optInFastDMAType, optInTimePeriod, optInFastKPeriod, optInFastDPeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }
            
            outBegIdx = startIdx;
            int tempArraySize = endIdx - startIdx + 1 + lookbackSTOCHF;
            var tempRSIBuffer = BufferHelpers.New(tempArraySize);
            RetCode retCode = Rsi(ref inReal, startIdx - lookbackSTOCHF, endIdx, ref tempRSIBuffer, out _, out var outNbElement1, optInTimePeriod);
            if (retCode != RetCode.Success || outNbElement1 == 0)
            {
                return retCode;
            }
            
            retCode = StochF(ref tempRSIBuffer, ref tempRSIBuffer, ref tempRSIBuffer, 0, tempArraySize - 1, ref outFastK, ref outFastD, out _, out outNbElement,
                optInFastDMAType, optInFastKPeriod, optInFastDPeriod);
            if (retCode != RetCode.Success || outNbElement == 0)
            {
                return retCode;
            }

            return RetCode.Success;
        }

        public static int StochRsiLookback(MAType optInFastDMAType = MAType.Sma, int optInTimePeriod = 14, int optInFastKPeriod = 5,
            int optInFastDPeriod = 3)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000 || optInFastKPeriod < 1 || optInFastKPeriod > 100000 ||
                optInFastDPeriod < 1 || optInFastDPeriod > 100000)
            {
                return -1;
            }

            return RsiLookback(optInTimePeriod) + StochFLookback(optInFastDMAType, optInFastKPeriod, optInFastDPeriod);
        }
    }
}
