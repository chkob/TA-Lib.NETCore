using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        // TODO: Find unit test data
        public static RetCode MacdFix(
            ref Span<decimal> input,
            ref Span<decimal> output,
            int inputSize,
            out int outputSize,
            int optInSignalPeriod = 9)
        {
            if (optInSignalPeriod < 1 || optInSignalPeriod > 100000)
            {
                outputSize = 0;
                return RetCode.BadParam;
            }

            return INT_MACD(
                ref input,
                ref output,
                inputSize,
                out outputSize,
                0,
                0,
                optInSignalPeriod);
        }

        public static int MacdFixLookback(int optInSignalPeriod = 9) => EmaLookback(26) + EmaLookback(optInSignalPeriod);
    }
}
