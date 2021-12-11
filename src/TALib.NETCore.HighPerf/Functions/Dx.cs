using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Dx(
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

            if (inHigh == null || inLow == null || inClose == null || outReal == null || optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = DxLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            outBegIdx = startIdx;
            decimal prevMinusDM = default;
            decimal prevPlusDM = default;
            decimal prevTR = default;
            int today = startIdx - lookbackTotal;
            decimal prevHigh = inHigh[today];
            decimal prevLow = inLow[today];
            decimal prevClose = inClose[today];
            int i = optInTimePeriod - 1;
            while (i-- > 0)
            {
                today++;
                decimal tempReal = inHigh[today];
                decimal diffP = tempReal - prevHigh;
                prevHigh = tempReal;

                tempReal = inLow[today];
                decimal diffM = prevLow - tempReal;
                prevLow = tempReal;

                if (diffM > 0.0m && diffP < diffM)
                {
                    prevMinusDM += diffM;
                }
                else if (diffP > 0.0m && diffP > diffM)
                {
                    prevPlusDM += diffP;
                }

                HighPerf.Lib.TrueRange(prevHigh, prevLow, prevClose, out tempReal);
                prevTR += tempReal;
                prevClose = inClose[today];
            }

            i = (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.Dx] + 1;
            while (i-- != 0)
            {
                today++;
                decimal tempReal = inHigh[today];
                decimal diffP = tempReal - prevHigh;
                prevHigh = tempReal;

                tempReal = inLow[today];
                decimal diffM = prevLow - tempReal;
                prevLow = tempReal;

                prevMinusDM -= prevMinusDM / optInTimePeriod;
                prevPlusDM -= prevPlusDM / optInTimePeriod;

                if (diffM > 0.0m && diffP < diffM)
                {
                    prevMinusDM += diffM;
                }
                else if (diffP > 0.0m && diffP > diffM)
                {
                    prevPlusDM += diffP;
                }

                HighPerf.Lib.TrueRange(prevHigh, prevLow, prevClose, out tempReal);
                prevTR = prevTR - prevTR / optInTimePeriod + tempReal;
                prevClose = inClose[today];
            }

            if (!HighPerf.Lib.IsZero(prevTR))
            {
                decimal minusDI = 100.0m * (prevMinusDM / prevTR);
                decimal plusDI = 100.0m * (prevPlusDM / prevTR);
                decimal tempReal = minusDI + plusDI;
                outReal[0] = !HighPerf.Lib.IsZero(tempReal) ? 100.0m * (Math.Abs(minusDI - plusDI) / tempReal) : 0.0m;
            }
            else
            {
                outReal[0] = 0.0m;
            }

            var outIdx = 1;
            while (today < endIdx)
            {
                today++;
                decimal tempReal = inHigh[today];
                decimal diffP = tempReal - prevHigh;
                prevHigh = tempReal;

                tempReal = inLow[today];
                decimal diffM = prevLow - tempReal;
                prevLow = tempReal;

                prevMinusDM -= prevMinusDM / optInTimePeriod;
                prevPlusDM -= prevPlusDM / optInTimePeriod;

                if (diffM > 0.0m && diffP < diffM)
                {
                    prevMinusDM += diffM;
                }
                else if (diffP > 0.0m && diffP > diffM)
                {
                    prevPlusDM += diffP;
                }

                HighPerf.Lib.TrueRange(prevHigh, prevLow, prevClose, out tempReal);
                prevTR = prevTR - prevTR / optInTimePeriod + tempReal;
                prevClose = inClose[today];

                if (!HighPerf.Lib.IsZero(prevTR))
                {
                    decimal minusDI = 100.0m * (prevMinusDM / prevTR);
                    decimal plusDI = 100.0m * (prevPlusDM / prevTR);
                    tempReal = minusDI + plusDI;
                    if (!HighPerf.Lib.IsZero(tempReal))
                    {
                        outReal[outIdx] = 100.0m * (Math.Abs(minusDI - plusDI) / tempReal);
                    }
                    else
                    {
                        outReal[outIdx] = outReal[outIdx - 1];
                    }
                }
                else
                {
                    outReal[outIdx] = outReal[outIdx - 1];
                }

                outIdx++;
            }

            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int DxLookback(int optInTimePeriod = 14)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod + (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.Dx];
        }
    }
}
