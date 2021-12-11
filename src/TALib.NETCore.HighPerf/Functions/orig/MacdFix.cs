using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode MacdFix(
            ref Span<double> inReal,
            int startIdx,
            int endIdx,
            ref Span<double> outMacd,
            ref Span<double> outMacdSignal,
            ref Span<double> outMacdHist,
            out int outBegIdx,
            out int outNbElement,
            int optInSignalPeriod = 9)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inReal == null || outMacd == null || outMacdSignal == null || outMacdHist == null || optInSignalPeriod < 1 ||
                optInSignalPeriod > 100000)
            {
                return RetCode.BadParam;
            }

            return HighPerf.Lib.INT_MACD(ref inReal, startIdx, endIdx, ref outMacd, ref outMacdSignal, ref outMacdHist, out outBegIdx, out outNbElement, 0, 0,
                optInSignalPeriod);
        }
        
        public static int MacdFixLookback(int optInSignalPeriod = 9) => EmaLookback(26) + EmaLookback(optInSignalPeriod);
    }
}
