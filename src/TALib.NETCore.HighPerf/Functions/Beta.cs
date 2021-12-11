using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Beta(
            ref Span<decimal> inReal0,
            ref Span<decimal> inReal1,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
            out int outBegIdx,
            out int outNbElement,
            int optInTimePeriod = 5)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inReal0 == null || inReal1 == null || outReal == null || optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = BetaLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            decimal x, y, tmpReal, sxy, sx, sy;
            decimal sxx = sxy = sx = sy = default;
            int trailingIdx = startIdx - lookbackTotal;
            var trailingLastPriceX = inReal0[trailingIdx];
            var lastPriceX = trailingLastPriceX;
            var trailingLastPriceY = inReal1[trailingIdx];
            var lastPriceY = trailingLastPriceY;

            int i = ++trailingIdx;
            while (i < startIdx)
            {
                tmpReal = inReal0[i];
                x = !HighPerf.Lib.IsZero(lastPriceX) ? (tmpReal - lastPriceX) / lastPriceX : 0.0m;
                lastPriceX = tmpReal;

                tmpReal = inReal1[i++];
                y = !HighPerf.Lib.IsZero(lastPriceY) ? (tmpReal - lastPriceY) / lastPriceY : 0.0m;
                lastPriceY = tmpReal;

                sxx += x * x;
                sxy += x * y;
                sx += x;
                sy += y;
            }

            int outIdx = default;
            do
            {
                tmpReal = inReal0[i];
                x = !HighPerf.Lib.IsZero(lastPriceX) ? (tmpReal - lastPriceX) / lastPriceX : 0.0m;
                lastPriceX = tmpReal;

                tmpReal = inReal1[i++];
                y = !HighPerf.Lib.IsZero(lastPriceY) ? (tmpReal - lastPriceY) / lastPriceY : 0.0m;
                lastPriceY = tmpReal;

                sxx += x * x;
                sxy += x * y;
                sx += x;
                sy += y;

                tmpReal = inReal0[trailingIdx];
                x = !HighPerf.Lib.IsZero(trailingLastPriceX) ? (tmpReal - trailingLastPriceX) / trailingLastPriceX : 0.0m;
                trailingLastPriceX = tmpReal;

                tmpReal = inReal1[trailingIdx++];
                y = !HighPerf.Lib.IsZero(trailingLastPriceY) ? (tmpReal - trailingLastPriceY) / trailingLastPriceY : 0.0m;
                trailingLastPriceY = tmpReal;

                tmpReal = optInTimePeriod * sxx - sx * sx;
                outReal[outIdx++] = !HighPerf.Lib.IsZero(tmpReal) ? (optInTimePeriod * sxy - sx * sy) / tmpReal : 0.0m;

                sxx -= x * x;
                sxy -= x * y;
                sx -= x;
                sy -= y;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }
        
        public static int BetaLookback(int optInTimePeriod = 5)
        {
            if (optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod;
        }
    }
}
