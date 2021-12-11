using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Ad(
            ref Span<double> inHigh,
            ref Span<double> inLow,
            ref Span<double> inClose,
            ref Span<double> inVolume,
            int startIdx,
            int endIdx,
            ref Span<double> outReal,
            out int outBegIdx,
            out int outNbElement)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inHigh == null || inLow == null || inClose == null || inVolume == null || outReal == null)
            {
                return RetCode.BadParam;
            }

            int nbBar = endIdx - startIdx + 1;
            outBegIdx = startIdx;
            outNbElement = nbBar;
            int currentBar = startIdx;
            int outIdx = default;
            double ad = default;
            while (nbBar != 0)
            {
                double high = inHigh[currentBar];
                double low = inLow[currentBar];
                double tmp = high - low;
                double close = inClose[currentBar];

                if (tmp > 0.0)
                {
                    ad += (close - low - (high - close)) / tmp * inVolume[currentBar];
                }

                outReal[outIdx++] = ad;

                currentBar++;
                nbBar--;
            }

            return RetCode.Success;
        }

        public static int AdLookback() => 0;
    }
}
