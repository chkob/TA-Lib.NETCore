using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Ppo(
            ref Span<double> inReal,
            int startIdx,
            int endIdx,
            ref Span<double> outReal,
            out int outBegIdx,
            out int outNbElement,
            MAType optInMAType = MAType.Sma,
            int optInFastPeriod = 12,
            int optInSlowPeriod = 26)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inReal == null || outReal == null || optInFastPeriod < 2 || optInFastPeriod > 100000 || optInSlowPeriod < 2 ||
                optInSlowPeriod > 100000)
            {
                return RetCode.BadParam;
            }

            var tempBuffer = BufferHelpers.New(endIdx - startIdx + 1);

            return HighPerf.Lib.INT_PO(ref inReal, startIdx, endIdx, ref outReal, out outBegIdx, out outNbElement, optInFastPeriod, optInSlowPeriod,
                optInMAType, ref tempBuffer, true);
        }

        public static int PpoLookback(MAType optInMAType = MAType.Sma, int optInFastPeriod = 12, int optInSlowPeriod = 26)
        {
            if (optInFastPeriod < 2 || optInFastPeriod > 100000 || optInSlowPeriod < 2 || optInSlowPeriod > 100000)
            {
                return -1;
            }

            return MaLookback(optInMAType, Math.Max(optInSlowPeriod, optInFastPeriod));
        }
    }
}
