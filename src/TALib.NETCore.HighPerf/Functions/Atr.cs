using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Atr(
            ref Span<decimal> input,
            ref Span<decimal> output,
            int inputSize,
            out int outputSize,
            //ref Span<decimal> inHigh,
            //ref Span<decimal> inLow,
            //ref Span<decimal> inClose,
            //int startIdx,
            //int endIdx,
            //ref Span<decimal> outReal,
            //out int outBegIdx,
            //out int outNbElement,
            int optInTimePeriod = 14)
        {
            var startIdx = 0;
            var endIdx = inputSize - 1;

            var inHigh = input.Series(inputSize, 0);
            var inLow = input.Series(inputSize, 1);
            var inClose = input.Series(inputSize, 2);

            if (optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                outputSize = 0;
                return RetCode.BadParam;
            }

            var lookbackTotal = AtrLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                outputSize = 0;
                return RetCode.Success;
            }

            if (optInTimePeriod == 1)
            {
                return TRange(ref inHigh, ref inLow, ref inClose, startIdx, endIdx, ref output, out var outBegIdx, out outputSize);
            }

            var prevATRTemp = BufferHelpers.New(1);
            var tempBuffer = BufferHelpers.New(lookbackTotal + (endIdx - startIdx) + 1);
            RetCode retCode = TRange(ref inHigh, ref inLow, ref inClose, startIdx - lookbackTotal + 1, endIdx, ref tempBuffer, out _, out _);
            if (retCode != RetCode.Success)
            {
                outputSize = 0;
                return retCode;
            }

            retCode = INT_SMA(ref tempBuffer, optInTimePeriod - 1, optInTimePeriod - 1, ref prevATRTemp, out _, out _, optInTimePeriod);
            if (retCode != RetCode.Success)
            {
                outputSize = 0;
                return retCode;
            }

            var prevATR = prevATRTemp[0];
            var today = optInTimePeriod;
            var outIdx = (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.Atr];
            while (outIdx != 0)
            {
                prevATR *= optInTimePeriod - 1;
                prevATR += tempBuffer[today++];
                prevATR /= optInTimePeriod;
                outIdx--;
            }

            outIdx = 1;
            output[0] = prevATR;

            int nbATR = endIdx - startIdx + 1;

            while (--nbATR != 0)
            {
                prevATR *= optInTimePeriod - 1;
                prevATR += tempBuffer[today++];
                prevATR /= optInTimePeriod;
                output[outIdx++] = prevATR;
            }

         
            outputSize = outIdx;
            output = output.Slice(0, outputSize);
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
