using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Aroon(
            ref Span<double> input,
            ref Span<double> output,
            int inputSize,
            out int outputSize,
            int optInTimePeriod = 14)
        {
            var startIdx = 0;
            var endIdx = inputSize - 1;

            var inHigh = input.Series(inputSize, 0);
            var inLow = input.Series(inputSize, 1);

            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                outputSize = 0;
                return RetCode.BadParam;
            }

            var lookbackTotal = AroonLookback(optInTimePeriod);
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
            var lowestIdx = -1;
            var highestIdx = -1;
            var lowest = 0.0;
            var highest = 0.0;
            var factor = 100.0 / optInTimePeriod;

            outputSize = endIdx - today + 1;
            var outAroonDown = output.Series(outputSize, 0);
            var outAroonUp = output.Series(outputSize, 1);

            var tmp = inLow[today];
            while (today <= endIdx)
            {
                if (lowestIdx < trailingIdx)
                {
                    lowestIdx = trailingIdx;
                    lowest = inLow[lowestIdx];
                    var i = lowestIdx;
                    while (++i <= today)
                    {
                        tmp = inLow[i];
                        if (tmp <= lowest)
                        {
                            lowestIdx = i;
                            lowest = tmp;
                        }
                    }
                }
                else if (tmp <= lowest)
                {
                    lowestIdx = today;
                    lowest = tmp;
                }

                tmp = inHigh[today];
                if (highestIdx < trailingIdx)
                {
                    highestIdx = trailingIdx;
                    highest = inHigh[highestIdx];
                    var i = highestIdx;
                    while (++i <= today)
                    {
                        tmp = inHigh[i];
                        if (tmp >= highest)
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
                
                outAroonUp[outIdx] = factor * (optInTimePeriod - (today - highestIdx));
                outAroonDown[outIdx] = factor * (optInTimePeriod - (today - lowestIdx));

                outIdx++;
                trailingIdx++;
                today++;
            }

            output = output.Slice(0, 2 * outputSize);
            return RetCode.Success;
        }

        public static int AroonLookback(int optInTimePeriod = 14)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod;
        }
    }
}
