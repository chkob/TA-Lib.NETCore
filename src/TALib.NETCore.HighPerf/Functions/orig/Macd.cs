using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Macd(
            ref Span<double> inReal,
            int startIdx,
            int endIdx,
            ref Span<double> outMacd,
            ref Span<double> outMacdSignal,
            ref Span<double> outMacdHist,
            out int outBegIdx,
            out int outNbElement,
            int optInFastPeriod = 12,
            int optInSlowPeriod = 26,
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

            return HighPerf.Lib.INT_MACD(ref inReal, startIdx, endIdx, ref outMacd, ref outMacdSignal, ref outMacdHist, out outBegIdx, out outNbElement,
                optInFastPeriod, optInSlowPeriod, optInSignalPeriod);
        }

        public static int Maookback(int optInFastPeriod = 12, int optInSlowPeriod = 26, int optInSignalPeriod = 9)
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
