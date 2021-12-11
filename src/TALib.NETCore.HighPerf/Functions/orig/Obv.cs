using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Obv(
            ref Span<double> inReal,
            ref Span<double> inVolume,
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

            if (inReal == null || inVolume == null || outReal == null)
            {
                return RetCode.BadParam;
            }

            double prevOBV = inVolume[startIdx];
            double prevReal = inReal[startIdx];
            int outIdx = default;

            for (int i = startIdx; i <= endIdx; i++)
            {
                double tempReal = inReal[i];
                if (tempReal > prevReal)
                {
                    prevOBV += inVolume[i];
                }
                else if (tempReal < prevReal)
                {
                    prevOBV -= inVolume[i];
                }

                outReal[outIdx++] = prevOBV;
                prevReal = tempReal;
            }

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int ObvLookback() => 0;
    }
}
