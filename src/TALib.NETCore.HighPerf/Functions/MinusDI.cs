using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode MinusDI(
            ref Span<decimal> inHigh,
            ref Span<decimal> inLow,
            ref Span<decimal> inClose,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
            out int outBegIdx,
            out int outNbElement,
            int optInTimePeriod = 14)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inHigh == null || inLow == null || inClose == null || outReal == null || optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = MinusDILookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            int today;
            decimal prevLow;
            decimal prevHigh;
            decimal diffM;
            decimal prevClose;
            decimal diffP;
            int outIdx = default;
            if (optInTimePeriod == 1)
            {
                outBegIdx = startIdx;
                today = startIdx - 1;
                prevHigh = inHigh[today];
                prevLow = inLow[today];
                prevClose = inClose[today];
                while (today < endIdx)
                {
                    today++;
                    decimal tempReal = inHigh[today];
                    diffP = tempReal - prevHigh;
                    prevHigh = tempReal;
                    tempReal = inLow[today];
                    diffM = prevLow - tempReal;
                    prevLow = tempReal;
                    if (diffM > 0.0m && diffP < diffM)
                    {
                        HighPerf.Lib.TrueRange(prevHigh, prevLow, prevClose, out tempReal);
                        outReal[outIdx++] = !HighPerf.Lib.IsZero(tempReal) ? diffM / tempReal : 0.0m;
                    }
                    else
                    {
                        outReal[outIdx++] = 0.0m;
                    }

                    prevClose = inClose[today];
                }

                outNbElement = outIdx;

                return RetCode.Success;
            }

            today = startIdx;
            outBegIdx = today;
            decimal prevMinusDM = default;
            decimal prevTR = default;
            today = startIdx - lookbackTotal;
            prevHigh = inHigh[today];
            prevLow = inLow[today];
            prevClose = inClose[today];
            int i = optInTimePeriod - 1;
            while (i-- > 0)
            {
                today++;
                decimal tempReal = inHigh[today];
                diffP = tempReal - prevHigh;
                prevHigh = tempReal;

                tempReal = inLow[today];
                diffM = prevLow - tempReal;
                prevLow = tempReal;
                if (diffM > 0.0m && diffP < diffM)
                {
                    prevMinusDM += diffM;
                }

                HighPerf.Lib.TrueRange(prevHigh, prevLow, prevClose, out tempReal);
                prevTR += tempReal;
                prevClose = inClose[today];
            }

            i = (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.MinusDI] + 1;
            while (i-- != 0)
            {
                today++;
                decimal tempReal = inHigh[today];
                diffP = tempReal - prevHigh;
                prevHigh = tempReal;
                tempReal = inLow[today];
                diffM = prevLow - tempReal;
                prevLow = tempReal;
                if (diffM > 0.0m && diffP < diffM)
                {
                    prevMinusDM = prevMinusDM - prevMinusDM / optInTimePeriod + diffM;
                }
                else
                {
                    prevMinusDM -= prevMinusDM / optInTimePeriod;
                }

                HighPerf.Lib.TrueRange(prevHigh, prevLow, prevClose, out tempReal);
                prevTR = prevTR - prevTR / optInTimePeriod + tempReal;
                prevClose = inClose[today];
            }

            outReal[0] = !HighPerf.Lib.IsZero(prevTR) ? 100.0m * (prevMinusDM / prevTR) : 0.0m;
            outIdx = 1;

            while (today < endIdx)
            {
                today++;
                decimal tempReal = inHigh[today];
                diffP = tempReal - prevHigh;
                prevHigh = tempReal;
                tempReal = inLow[today];
                diffM = prevLow - tempReal;
                prevLow = tempReal;
                if (diffM > 0.0m && diffP < diffM)
                {
                    prevMinusDM = prevMinusDM - prevMinusDM / optInTimePeriod + diffM;
                }
                else
                {
                    prevMinusDM -= prevMinusDM / optInTimePeriod;
                }

                HighPerf.Lib.TrueRange(prevHigh, prevLow, prevClose, out tempReal);
                prevTR = prevTR - prevTR / optInTimePeriod + tempReal;
                prevClose = inClose[today];
                outReal[outIdx++] = !HighPerf.Lib.IsZero(prevTR) ? 100.0m * (prevMinusDM / prevTR) : 0.0m;
            }

            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int MinusDILookback(int optInTimePeriod = 14)
        {
            if (optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod > 1 ? optInTimePeriod + (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.MinusDI] : 1;
        }
    }
}
