using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Macd(
            ref Span<double> input,
            ref Span<double> output,
            int inputSize,
            out int outputSize,
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

            return INT_MACD(
                ref input,
                ref output,
                inputSize,
                out outputSize, 
                optInFastPeriod,
                optInSlowPeriod,
                optInSignalPeriod);
        }

        public static int MacdLookback(int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
        {
            if (optInFastPeriod < 2 || optInFastPeriod > 100000 || optInSlowPeriod < 2 || optInSlowPeriod > 100000 ||
                optInSignalPeriod < 1 || optInSignalPeriod > 100000)
            {
                return -1;
            }

            if (optInSlowPeriod < optInFastPeriod)
            {
                optInSlowPeriod = optInFastPeriod;
            }

            return EmaLookback(optInSlowPeriod) + EmaLookback(optInSignalPeriod);
        }
    }
}
