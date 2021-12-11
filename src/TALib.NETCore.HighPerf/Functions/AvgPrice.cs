using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode AvgPrice(
            ref Span<decimal> input,
            ref Span<decimal> output,
            int inputSize,
            out int outputSize)
        {
            var startIdx = 0;
            var endIdx = inputSize - 1;

            var inOpen = input.Series(inputSize, 0);
            var inHigh = input.Series(inputSize, 1);
            var inLow = input.Series(inputSize, 2);
            var inClose = input.Series(inputSize, 3);

            var outIdx = 0;
            for (var i = startIdx; i <= endIdx; i++)
            {
                output[outIdx++] = (inHigh[i] + inLow[i] + inClose[i] + inOpen[i]) / 4.0m;
            }

            outputSize = outIdx;
            output = output.Slice(0, outputSize);
            return RetCode.Success;
        }

        public static int AvgPriceLookback() => 0;
    }
}
