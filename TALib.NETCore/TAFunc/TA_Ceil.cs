using System;

namespace TALib
{
    public partial class Core
    {
        public static RetCode Ceil(int startIdx, int endIdx, double[] inReal, ref int outBegIdx, ref int outNBElement, double[] outReal)
        {
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
                outReal[outIdx++] = Math.Ceiling(inReal[i]);
            }

            outNBElement = outIdx;
            outBegIdx = startIdx;

            return RetCode.Success;
        }

        public static RetCode Ceil(int startIdx, int endIdx, decimal[] inReal, ref int outBegIdx, ref int outNBElement, decimal[] outReal)
        {
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
                outReal[outIdx++] = Math.Ceiling(inReal[i]);
            }

            outNBElement = outIdx;
            outBegIdx = startIdx;

            return RetCode.Success;
        }

        public static int CeilLookback()
        {
            return 0;
        }
    }
}
