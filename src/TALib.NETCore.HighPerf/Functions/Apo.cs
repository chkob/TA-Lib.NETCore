using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Apo(
            ref Span<decimal> input,
            ref Span<decimal> output,
            int inputSize,
            out int outputSize,
            MAType optInMAType = MAType.Sma,
            int optInFastPeriod = 12,
            int optInSlowPeriod = 26)
        {
            if (optInFastPeriod < 2 || optInFastPeriod > 100000 ||
                optInSlowPeriod < 2 || optInSlowPeriod > 100000)
            {
                outputSize = 0;
                return RetCode.BadParam;
            }

            var startIdx = 0;
            var endIdx = inputSize - 1;
            var tempBuffer = BufferHelpers.New(inputSize);

            var result = INT_PO(
                ref input,
                startIdx,
                endIdx,
                ref output,
                out _,
                out outputSize,
                optInFastPeriod,
                optInSlowPeriod,
                optInMAType,
                ref tempBuffer,
                false);

            output = output.Slice(0, outputSize);
            return result;
        }

        public static int ApoLookback(MAType optInMAType = MAType.Sma, int optInFastPeriod = 12, int optInSlowPeriod = 26)
        {
            if (optInFastPeriod < 2 || optInFastPeriod > 100000 || optInSlowPeriod < 2 || optInSlowPeriod > 100000)
            {
                return -1;
            }

            return MaLookback(optInMAType, Math.Max(optInSlowPeriod, optInFastPeriod));
        }
    }
}
