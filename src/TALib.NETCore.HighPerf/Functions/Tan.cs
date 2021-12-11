using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Tan(
            ref Span<decimal> inReal,
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

            if (inReal == null || outReal == null)
            {
                return RetCode.BadParam;
            }

            int outIdx = default;
            for (int i = startIdx; i <= endIdx; i++)
            {
                outReal[outIdx++] = (decimal) Math.Tan((double)inReal[i]);
            }

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int TanLookback() => 0;
    }
}
