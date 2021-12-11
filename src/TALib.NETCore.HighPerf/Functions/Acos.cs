using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Acos(
            ref Span<decimal> input,
            ref Span<decimal> output,
            int inputSize,
            out int outputSize)
        {
            for (var i = 0; i < inputSize; i++)
            {
                output[i] = (decimal)Math.Acos((double)input[i]);
            }

            outputSize = inputSize;
            output = output.Slice(0, outputSize);
            return RetCode.Success;
        }

        public static int AcosLookback() => 0;
    }
}
