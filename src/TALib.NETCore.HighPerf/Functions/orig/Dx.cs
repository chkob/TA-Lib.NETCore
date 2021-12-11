using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Dx(
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
            double prevMinusDM = default;
            double prevPlusDM = default;
            double prevTR = default;
            int today = startIdx - lookbackTotal;
            double prevHigh = inHigh[today];
            double prevLow = inLow[today];
            double prevClose = inClose[today];
            int i = optInTimePeriod - 1;
            while (i-- > 0)
            {
                today++;
                double tempReal = inHigh[today];
                double diffP = tempReal - prevHigh;
                prevHigh = tempReal;

                tempReal = inLow[today];
                double diffM = prevLow - tempReal;
                prevLow = tempReal;

                if (diffM > 0.0 && diffP < diffM)
                {
                    prevMinusDM += diffM;
                }
                else if (diffP > 0.0 && diffP > diffM)
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
                double tempReal = inHigh[today];
                double diffP = tempReal - prevHigh;
                prevHigh = tempReal;

                tempReal = inLow[today];
                double diffM = prevLow - tempReal;
                prevLow = tempReal;

                prevMinusDM -= prevMinusDM / optInTimePeriod;
                prevPlusDM -= prevPlusDM / optInTimePeriod;

                if (diffM > 0.0 && diffP < diffM)
                {
                    prevMinusDM += diffM;
                }
                else if (diffP > 0.0 && diffP > diffM)
                {
                    prevPlusDM += diffP;
                }

                HighPerf.Lib.TrueRange(prevHigh, prevLow, prevClose, out tempReal);
                prevTR = prevTR - prevTR / optInTimePeriod + tempReal;
                prevClose = inClose[today];
            }

            if (!HighPerf.Lib.IsZero(prevTR))
            {
                double minusDI = 100.0 * (prevMinusDM / prevTR);
                double plusDI = 100.0 * (prevPlusDM / prevTR);
                double tempReal = minusDI + plusDI;
                outReal[0] = !HighPerf.Lib.IsZero(tempReal) ? 100.0 * (Math.Abs(minusDI - plusDI) / tempReal) : 0.0;
            }
            else
            {
                outReal[0] = 0.0;
            }

            var outIdx = 1;
            while (today < endIdx)
            {
                today++;
                double tempReal = inHigh[today];
                double diffP = tempReal - prevHigh;
                prevHigh = tempReal;

                tempReal = inLow[today];
                double diffM = prevLow - tempReal;
                prevLow = tempReal;

                prevMinusDM -= prevMinusDM / optInTimePeriod;
                prevPlusDM -= prevPlusDM / optInTimePeriod;

                if (diffM > 0.0 && diffP < diffM)
                {
                    prevMinusDM += diffM;
                }
                else if (diffP > 0.0 && diffP > diffM)
                {
                    prevPlusDM += diffP;
                }

                HighPerf.Lib.TrueRange(prevHigh, prevLow, prevClose, out tempReal);
                prevTR = prevTR - prevTR / optInTimePeriod + tempReal;
                prevClose = inClose[today];

                if (!HighPerf.Lib.IsZero(prevTR))
                {
                    double minusDI = 100.0 * (prevMinusDM / prevTR);
                    double plusDI = 100.0 * (prevPlusDM / prevTR);
                    tempReal = minusDI + plusDI;
                    if (!HighPerf.Lib.IsZero(tempReal))
                    {
                        outReal[outIdx] = 100.0 * (Math.Abs(minusDI - plusDI) / tempReal);
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
