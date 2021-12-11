using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode StdDev(
            ref Span<double> inReal,
            int startIdx,
            int endIdx,
            ref Span<double> outReal,
            out int outBegIdx,
            out int outNbElement,
            int optInTimePeriod = 5,
            double optInNbDev = 1.0)
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
                    double tempReal = outReal[i];
                    outReal[i] = !HighPerf.Lib.IsZeroOrNeg(tempReal) ? Math.Sqrt(tempReal) * optInNbDev : 0.0;
                }
            }
            else
            {
                for (var i = 0; i < outNbElement; i++)
                {
                    double tempReal = outReal[i];
                    outReal[i] = !HighPerf.Lib.IsZeroOrNeg(tempReal) ? Math.Sqrt(tempReal) : 0.0;
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
