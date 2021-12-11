using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Ad(
            ref Span<double> input,
            ref Span<double> output,
            int inputSize,
            out int outputSize)
        {
            var inHigh = input.Series(inputSize, 0);
            var inLow = input.Series(inputSize, 1);
            var inClose = input.Series(inputSize, 2);
            var inVolume = input.Series(inputSize, 3);
            var outReal = output.Series(inputSize, 0);

            var startIdx = 0;
            var endIdx = inputSize - 1;
            var nbBar = endIdx - startIdx + 1;
            var currentBar = startIdx;
            var outIdx = 0;
            var ad = 0.0;

            while (nbBar != 0)
            {
                var high = inHigh[currentBar];
                var low = inLow[currentBar];
                var tmp = high - low;
                var close = inClose[currentBar];

                if (tmp > 0.0)
                {
                    ad += (close - low - (high - close)) / tmp * inVolume[currentBar];
                }

                outReal[outIdx++] = ad;

                currentBar++;
                nbBar--;
            }

            outputSize = inputSize;
            output = output.Slice(0, outputSize);
            return RetCode.Success;
        }

        public static int AdLookback() => 0;
    }
}
