using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Atr(
            ref Span<double> inHigh,
            ref Span<double> inLow,
            ref Span<double> inClose,
            int startIdx,
            int endIdx,
            ref Span<double> outReal,
            out int outBegIdx,
            out int outNbElement,
            int optInTimePeriod = 14)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inHigh == null || inLow == null || inClose == null || outReal == null || optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = AtrLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            if (optInTimePeriod == 1)
            {
                return TRange(ref inHigh, ref inLow, ref inClose, startIdx, endIdx, ref outReal, out outBegIdx, out outNbElement);
            }

            var prevATRTemp = BufferHelpers.New(1);
            var tempBuffer = BufferHelpers.New(lookbackTotal + (endIdx - startIdx) + 1);
            RetCode retCode = TRange(ref inHigh, ref inLow, ref inClose, startIdx - lookbackTotal + 1, endIdx, ref tempBuffer, out _, out _);
            if (retCode != RetCode.Success)
            {
                return retCode;
            }

            retCode = HighPerf.Lib.INT_SMA(ref tempBuffer, optInTimePeriod - 1, optInTimePeriod - 1, ref prevATRTemp, out _, out _, optInTimePeriod);
            if (retCode != RetCode.Success)
            {
                return retCode;
            }

            double prevATR = prevATRTemp[0];
            int today = optInTimePeriod;
            int outIdx = (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.Atr];
            while (outIdx != 0)
            {
                prevATR *= optInTimePeriod - 1;
                prevATR += tempBuffer[today++];
                prevATR /= optInTimePeriod;
                outIdx--;
            }

            outIdx = 1;
            outReal[0] = prevATR;

            int nbATR = endIdx - startIdx + 1;

            while (--nbATR != 0)
            {
                prevATR *= optInTimePeriod - 1;
                prevATR += tempBuffer[today++];
                prevATR /= optInTimePeriod;
                outReal[outIdx++] = prevATR;
            }

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return retCode;
        }

        public static int AtrLookback(int optInTimePeriod = 14)
        {
            if (optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod + (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.Atr];
        }
    }
}
