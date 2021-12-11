using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Bop(
            ref Span<double> inOpen,
            ref Span<double> inHigh,
            ref Span<double> inLow,
            ref Span<double> inClose,
            int startIdx,
            int endIdx,
            ref Span<double> outReal,
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
                double tempReal = inHigh[i] - inLow[i];
                outReal[outIdx++] = !HighPerf.Lib.IsZeroOrNeg(tempReal) ? (inClose[i] - inOpen[i]) / tempReal : 0.0;
            }

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int BopLookback() => 0;
    }
}
