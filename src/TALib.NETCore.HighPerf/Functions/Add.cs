using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Add(
            ref Span<decimal> input,
            ref Span<decimal> output,
            int inputSize,
            out int outputSize)
        {
            var inReal0 = input.Series(inputSize, 0);
            var inReal1 = input.Series(inputSize, 1);

            for (var i = 0; i < inputSize; i++)
            {
                output[i] = inReal0[i] + inReal1[i];
            }

            outputSize = inputSize;
            output = output.Slice(0, outputSize);
            return RetCode.Success;
        }

        public static int AddLookback() => 0;
    }
}
