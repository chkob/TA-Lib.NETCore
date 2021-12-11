using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Cci(
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

            int lookbackTotal = CciLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            var circBuffer = BufferHelpers.New(optInTimePeriod);
            int circBufferIdx = default;
            var maxIdxCircBuffer = optInTimePeriod - 1;
            int i = startIdx - lookbackTotal;
            while (i < startIdx)
            {
                circBuffer[circBufferIdx++] = (inHigh[i] + inLow[i] + inClose[i]) / 3.0;
                i++;
                if (circBufferIdx > maxIdxCircBuffer)
                {
                    circBufferIdx = 0;
                }
            }

            int outIdx = default;
            do
            {
                double lastValue = (inHigh[i] + inLow[i] + inClose[i]) / 3.0;
                circBuffer[circBufferIdx++] = lastValue;

                double theAverage = default;
                for (var j = 0; j < optInTimePeriod; j++)
                {
                    theAverage += circBuffer[j];
                }
                theAverage /= optInTimePeriod;

                double tempReal2 = default;
                for (var j = 0; j < optInTimePeriod; j++)
                {
                    tempReal2 += Math.Abs(circBuffer[j] - theAverage);
                }

                double tempReal = lastValue - theAverage;
                outReal[outIdx++] = !tempReal.Equals(0.0) && !tempReal2.Equals(0.0)
                    ? tempReal / (0.015 * (tempReal2 / optInTimePeriod))
                    : 0.0;

                if (circBufferIdx > maxIdxCircBuffer)
                {
                    circBufferIdx = 0;
                }

                i++;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int CciLookback(int optInTimePeriod = 14)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod - 1;
        }
    }
}
