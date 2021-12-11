using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Bop(
            ref Span<decimal> inOpen,
            ref Span<decimal> inHigh,
            ref Span<decimal> inLow,
            ref Span<decimal> inClose,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
            out int outBegIdx,
            out int outNbElement)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inOpen == null || inHigh == null || inLow == null || inClose == null || outReal == null)
            {
                return RetCode.BadParam;
            }

            int outIdx = default;
            for (var i = startIdx; i <= endIdx; i++)
            {
                decimal tempReal = inHigh[i] - inLow[i];
                outReal[outIdx++] = !HighPerf.Lib.IsZeroOrNeg(tempReal) ? (inClose[i] - inOpen[i]) / tempReal : 0.0m;
            }

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int BopLookback() => 0;
    }
}
