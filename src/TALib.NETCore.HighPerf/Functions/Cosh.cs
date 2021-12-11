using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Cosh(
            ref Span<decimal> input,
            ref Span<decimal> output,
            int inputSize,
            out int outputSize)
        {
            for (var i = 0; i < inputSize; i++)
            {
                output[i] = (decimal)Math.Cosh((double)input[i]);
            }

            outputSize = inputSize;
            output = output.Slice(0, outputSize);
            return RetCode.Success;
        }

        public static int CoshLookback() => 0;
    }
}
