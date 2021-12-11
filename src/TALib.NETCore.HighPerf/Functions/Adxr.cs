using System;
using System.Xml;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Adxr(
            ref Span<double> input,
            ref Span<double> output,
            int inputSize,
            out int outputSize,
            int optInTimePeriod = 14)
        {
            var inHigh = input.Series(inputSize, 0);
            var inLow = input.Series(inputSize, 1);
            var inClose = input.Series(inputSize, 2);

            var startIdx = 0;
            var endIdx = inputSize - 1;

            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                outputSize = 0;
                return RetCode.BadParam;
            }

            var lookbackTotal = AdxrLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                outputSize = 0;
                return RetCode.Success;
            }

            var adx = BufferHelpers.New(endIdx - startIdx + optInTimePeriod);

            RetCode retCode = Adx(
                ref inHigh,
                ref inLow,
                ref inClose,
                startIdx - (optInTimePeriod - 1),
                endIdx,
                ref adx,
                out _,
                out _,
                optInTimePeriod);

            if (retCode != RetCode.Success)
            {
                outputSize = 0;
                return retCode;
            }

            var i = optInTimePeriod - 1;
            var j = 0;
            var outIdx = 0;
            var nbElement = endIdx - startIdx + 2;
            while (--nbElement != 0)
            {
                output[outIdx++] = (adx[i++] + adx[j++]) / 2.0;
            }
            
            outputSize = outIdx;
            output = output.Slice(0, outputSize);
            return RetCode.Success;
        }

        public static int AdxrLookback(int optInTimePeriod = 14)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod + AdxLookback(optInTimePeriod) - 1;
        }
    }
}
