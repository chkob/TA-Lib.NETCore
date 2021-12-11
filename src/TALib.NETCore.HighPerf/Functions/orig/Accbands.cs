using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Accbands(
            ref Span<double> inHigh,
            ref Span<double> inLow,
            ref Span<double> inClose,
            int startIdx,
            int endIdx,
            ref Span<double> outRealUpperBand,
            ref Span<double> outRealMiddleBand,
            ref Span<double> outRealLowerBand,
            out int outBegIdx,
            out int outNbElement,
            int optInTimePeriod = 20)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inHigh == null || inLow == null || inClose == null || outRealUpperBand == null || outRealMiddleBand == null ||
                outRealLowerBand == null || optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = AccbandsLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            int outputSize = endIdx - startIdx + 1;
            int bufferSize = outputSize + lookbackTotal;
            var tempBuffer1 = BufferHelpers.New(bufferSize);
            var tempBuffer2 = BufferHelpers.New(bufferSize);

            for (int j = 0, i = startIdx - lookbackTotal; i <= endIdx; i++, j++)
            {
                double tempReal = inHigh[i] + inLow[i];
                if (!HighPerf.Lib.IsZero(tempReal))
                {
                    tempReal = 4 * (inHigh[i] - inLow[i]) / tempReal;
                    tempBuffer1[j] = inHigh[i] * (1 + tempReal);
                    tempBuffer2[j] = inLow[i] * (1 - tempReal);
                }
                else
                {
                    tempBuffer1[j] = inHigh[i];
                    tempBuffer2[j] = inLow[i];
                }
            }

            var retCode = Sma(ref inClose, startIdx, endIdx, ref outRealMiddleBand, out _, out var outNbElementDummy, optInTimePeriod);
            if (retCode != RetCode.Success || outNbElementDummy != outputSize)
            {
                return retCode;
            }

            retCode = Sma(ref tempBuffer1, 0, bufferSize - 1, ref outRealUpperBand, out _, out outNbElementDummy, optInTimePeriod);
            if (retCode != RetCode.Success || outNbElementDummy != outputSize)
            {
                return retCode;
            }

            retCode = Sma(ref tempBuffer2, 0, bufferSize - 1, ref outRealLowerBand, out _, out outNbElementDummy, optInTimePeriod);
            if (retCode != RetCode.Success || outNbElementDummy != outputSize)
            {
                return retCode;
            }

            outBegIdx = startIdx;
            outNbElement = outputSize;

            return RetCode.Success;
        }

        public static int AccbandsLookback(int optInTimePeriod = 20)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return SmaLookback(optInTimePeriod);
        }
    }
}
