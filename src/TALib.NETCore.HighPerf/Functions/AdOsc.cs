using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode AdOsc(
            ref Span<decimal> input,
            ref Span<decimal> output,
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

            var ad = 0.0m;
            var fastk = 2.0m / (optInFastPeriod + 1);
            var oneMinusFastk = (decimal)(1.0m - fastk);
            var slowk = 2.0m / (optInSlowPeriod + 1);
            var oneMinusSlowk = (decimal)(1.0m - slowk);

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
            ref Span<decimal> inHigh,
            ref Span<decimal> inLow,
            ref Span<decimal> inClose,
            ref Span<decimal> inVolume,
            ref decimal ad,
            ref int today)
        {
            decimal h = inHigh[today];
            decimal l = inLow[today];
            decimal tmp = h - l;
            decimal c = inClose[today];
            if (tmp > 0.0m)
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
