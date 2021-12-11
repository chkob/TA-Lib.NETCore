using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Max(
            ref Span<decimal> input,
            ref Span<decimal> output,
            int inputSize,
            out int outputSize,
            int optInTimePeriod = 30)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                outputSize = 0;
                return RetCode.BadParam;
            }

            var startIdx = 0;
            var endIdx = inputSize - 1;

            var lookbackTotal = MaxLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                outputSize = 0;
                return RetCode.Success;
            }

            var outIdx = 0;
            var today = startIdx;
            var trailingIdx = startIdx - lookbackTotal;
            var highestIdx = -1;
            var highest = 0.0m;

            while (today <= endIdx)
            {
                var tmp = input[today];
                if (highestIdx < trailingIdx)
                {
                    highestIdx = trailingIdx;
                    highest = input[highestIdx];
                    var i = highestIdx;
                    while (++i <= today)
                    {
                        tmp = input[i];
                        if (tmp > highest)
                        {
                            highestIdx = i;
                            highest = tmp;
                        }
                    }
                }
                else if (tmp >= highest)
                {
                    highestIdx = today;
                    highest = tmp;
                }

                output[outIdx++] = highest;
                trailingIdx++;
                today++;
            }

            outputSize = outIdx;
            output = output.Slice(0, outputSize);
            return RetCode.Success;
        }

        public static int MaxLookback(int optInTimePeriod = 30)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod - 1;
        }
    }
}
