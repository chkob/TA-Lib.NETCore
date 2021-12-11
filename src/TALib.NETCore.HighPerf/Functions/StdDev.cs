using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode StdDev(
            ref Span<decimal> inReal,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
            out int outBegIdx,
            out int outNbElement,
            int optInTimePeriod = 5,
            decimal optInNbDev = 1.0m)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inReal == null || outReal == null || optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            RetCode retCode = HighPerf.Lib.INT_VAR(ref inReal, startIdx, endIdx, ref outReal, out outBegIdx, out outNbElement, optInTimePeriod);
            if (retCode != RetCode.Success)
            {
                return retCode;
            }

            if (!optInNbDev.Equals(1.0))
            {
                for (var i = 0; i < outNbElement; i++)
                {
                    decimal tempReal = outReal[i];
                    outReal[i] = !HighPerf.Lib.IsZeroOrNeg(tempReal) ? (decimal) Math.Sqrt((double)tempReal) * optInNbDev : 0.0m;
                }
            }
            else
            {
                for (var i = 0; i < outNbElement; i++)
                {
                    decimal tempReal = outReal[i];
                    outReal[i] = !HighPerf.Lib.IsZeroOrNeg(tempReal) ? (decimal) Math.Sqrt((double)tempReal) : 0.0m;
                }
            }

            return RetCode.Success;
        }

        public static int StdDevLookback(int optInTimePeriod = 5)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return VarLookback(optInTimePeriod);
        }
    }
}
