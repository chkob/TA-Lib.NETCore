using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Adxr(
            ref Span<double> inHigh,
            ref Span<double> inLow,
            ref Span<double> inClose,
            int startIdx,
            int endIdx,
            ref Span<double> outReal,
            out int outBegIdx,
            out int outNbElement,
            int optInTimePeriod = 14)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inHigh == null || inLow == null || inClose == null || outReal == null || optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = AdxrLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            var adx = BufferHelpers.New(endIdx - startIdx + optInTimePeriod);

            RetCode retCode = Adx(ref inHigh, ref inLow, ref inClose, startIdx - (optInTimePeriod - 1), endIdx, ref adx, out outBegIdx, out outNbElement,
                optInTimePeriod);
            if (retCode != RetCode.Success)
            {
                return retCode;
            }

            int i = optInTimePeriod - 1;
            int j = default;
            int outIdx = default;
            int nbElement = endIdx - startIdx + 2;
            while (--nbElement != 0)
            {
                outReal[outIdx++] = (adx[i++] + adx[j++]) / 2.0;
            }

            outBegIdx = startIdx;
            outNbElement = outIdx;

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
