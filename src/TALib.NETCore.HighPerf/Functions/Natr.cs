using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Natr(
            ref Span<decimal> inHigh,
            ref Span<decimal> inLow,
            ref Span<decimal> inClose,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
            out int outBegIdx, out int outNbElement, int optInTimePeriod = 14)
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

            int lookbackTotal = NatrLookback(optInTimePeriod);
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

            var tempBuffer = BufferHelpers.New(lookbackTotal + (endIdx - startIdx) + 1);
            RetCode retCode = TRange(ref inHigh, ref inLow, ref inClose, startIdx - lookbackTotal + 1, endIdx, ref tempBuffer, out _, out _);
            if (retCode != RetCode.Success)
            {
                return retCode;
            }

            var prevATRTemp = BufferHelpers.New(1);
            retCode = HighPerf.Lib.INT_SMA(ref tempBuffer, optInTimePeriod - 1, optInTimePeriod - 1, ref prevATRTemp, out _, out _, optInTimePeriod);
            if (retCode != RetCode.Success)
            {
                return retCode;
            }

            decimal prevATR = prevATRTemp[0];
            int today = optInTimePeriod;
            int outIdx = (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.Natr];
            while (outIdx != 0)
            {
                prevATR *= optInTimePeriod - 1;
                prevATR += tempBuffer[today++];
                prevATR /= optInTimePeriod;
                outIdx--;
            }

            outIdx = 1;
            decimal tempValue = inClose[today];
            outReal[0] = !HighPerf.Lib.IsZero(tempValue) ? prevATR / tempValue * 100.0m : 0.0m;

            int nbATR = endIdx - startIdx + 1;
            while (--nbATR != 0)
            {
                prevATR *= optInTimePeriod - 1;
                prevATR += tempBuffer[today++];
                prevATR /= optInTimePeriod;
                tempValue = inClose[today];
                if (!HighPerf.Lib.IsZero(tempValue))
                {
                    outReal[outIdx] = prevATR / tempValue * 100.0m;
                }
                else
                {
                    outReal[0] = 0.0m;
                }

                outIdx++;
            }

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return retCode;
        }

        public static int NatrLookback(int optInTimePeriod = 14)
        {
            if (optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod + (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.Natr];
        }
    }
}
