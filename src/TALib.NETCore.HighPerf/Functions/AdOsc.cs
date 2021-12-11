using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode AdOsc(
            ref Span<double> input,
            ref Span<double> output,
            int inputSize,
            out int outputSize,
            int optInFastPeriod = 3,
            int optInSlowPeriod = 10)
        {
            var inHigh = input.Series(inputSize, 0);
            var inLow = input.Series(inputSize, 1);
            var inClose = input.Series(inputSize, 2);
            var inVolume = input.Series(inputSize, 3);

            var startIdx = 0;
            var endIdx = inputSize - 1;
            
            if (optInFastPeriod < 2 || optInFastPeriod > 100000 ||
                optInSlowPeriod < 2 || optInSlowPeriod > 100000 )
            {
                outputSize = 0;
                return RetCode.BadParam;
            }

            int lookbackTotal = AdOscLookback(optInFastPeriod, optInSlowPeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                outputSize = 0;
                return RetCode.Success;
            }

            var today = startIdx - lookbackTotal;

            var ad = 0.0;
            var fastk = 2.0 / (optInFastPeriod + 1);
            var oneMinusFastk = 1.0 - fastk;
            var slowk = 2.0 / (optInSlowPeriod + 1);
            var oneMinusSlowk = 1.0 - slowk;

            CalculateAd(ref inHigh, ref inLow, ref inClose, ref inVolume, ref ad, ref today);
            var fastEMA = ad;
            var slowEMA = ad;

            while (today < startIdx)
            {
                CalculateAd(ref inHigh, ref inLow, ref inClose, ref inVolume, ref ad, ref today);
                fastEMA = fastk * ad + oneMinusFastk * fastEMA;
                slowEMA = slowk * ad + oneMinusSlowk * slowEMA;
            }

            int outIdx = default;
            while (today <= endIdx)
            {
                CalculateAd(ref inHigh, ref inLow, ref inClose, ref inVolume, ref ad, ref today);
                fastEMA = fastk * ad + oneMinusFastk * fastEMA;
                slowEMA = slowk * ad + oneMinusSlowk * slowEMA;

                output[outIdx++] = fastEMA - slowEMA;
            }

            outputSize = outIdx;
            output = output.Slice(0, outputSize);
            return RetCode.Success;
        }

        private static void CalculateAd(
            ref Span<double> inHigh,
            ref Span<double> inLow,
            ref Span<double> inClose,
            ref Span<double> inVolume,
            ref double ad,
            ref int today)
        {
            double h = inHigh[today];
            double l = inLow[today];
            double tmp = h - l;
            double c = inClose[today];
            if (tmp > 0.0)
            {
                ad += (c - l - (h - c)) / tmp * inVolume[today];
            }
            today++;
        }

        public static int AdOscLookback(int optInFastPeriod = 3, int optInSlowPeriod = 10)
        {
            if (optInFastPeriod < 2 || optInFastPeriod > 100000 || optInSlowPeriod < 2 || optInSlowPeriod > 100000)
            {
                return -1;
            }

            var slowestPeriod = optInFastPeriod < optInSlowPeriod ? optInSlowPeriod : optInFastPeriod;

            return EmaLookback(slowestPeriod);
        }
    }
}
