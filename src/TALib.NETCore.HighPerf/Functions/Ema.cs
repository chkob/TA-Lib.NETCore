using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Ema(
            ref Span<decimal> input,
            ref Span<decimal> output,
            int inputSize,
            out int outputSize,
            int optInTimePeriod = 30)
        {
            var result = Ema(ref input, 0, inputSize - 1, ref output, out _, out outputSize, optInTimePeriod);
            output = output.Slice(0, outputSize);
            return result;
        }

        internal static RetCode Ema(
            ref Span<decimal> inReal,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
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

            return HighPerf.Lib.INT_EMA(ref inReal, startIdx, endIdx, ref outReal, out outBegIdx, out outNbElement, optInTimePeriod,
                2.0m / (optInTimePeriod + 1));
        }

        public static int EmaLookback(
            int optInTimePeriod = 30)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod - 1 + (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.Ema];
        }
    }
}
