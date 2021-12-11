using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Rsi(
            ref Span<decimal> inReal,
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

            if (inReal == null || outReal == null || optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = RsiLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            int outIdx = default;
            int today = startIdx - lookbackTotal;
            decimal prevValue = inReal[today];
            decimal prevGain;
            decimal prevLoss;
            if (HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.Rsi] == 0 && HighPerf.Lib.Globals.Compatibility == Compatibility.Metastock)
            {
                decimal savePrevValue = prevValue;
                decimal tempValue1;
                decimal tempValue2;
                prevGain = default;
                prevLoss = default;
                for (int i = optInTimePeriod; i > 0; i--)
                {
                    tempValue1 = inReal[today++];
                    tempValue2 = tempValue1 - prevValue;
                    prevValue = tempValue1;
                    if (tempValue2 < 0.0m)
                    {
                        prevLoss -= tempValue2;
                    }
                    else
                    {
                        prevGain += tempValue2;
                    }
                }

                tempValue1 = prevLoss / optInTimePeriod;
                tempValue2 = prevGain / optInTimePeriod;

                tempValue1 = tempValue2 + tempValue1;
                outReal[outIdx++] = !HighPerf.Lib.IsZero(tempValue1) ? 100.0m * (tempValue2 / tempValue1) : 0.0m;

                if (today > endIdx)
                {
                    outBegIdx = startIdx;
                    outNbElement = outIdx;
                    return RetCode.Success;
                }

                today -= optInTimePeriod;
                prevValue = savePrevValue;
            }

            prevGain = default;
            prevLoss = default;
            today++;
            for (int i = optInTimePeriod; i > 0; i--)
            {
                decimal tempValue1 = inReal[today++];
                decimal tempValue2 = tempValue1 - prevValue;
                prevValue = tempValue1;
                if (tempValue2 < 0.0m)
                {
                    prevLoss -= tempValue2;
                }
                else
                {
                    prevGain += tempValue2;
                }
            }

            prevLoss /= optInTimePeriod;
            prevGain /= optInTimePeriod;

            if (today > startIdx)
            {
                decimal tempValue1 = prevGain + prevLoss;
                outReal[outIdx++] = !HighPerf.Lib.IsZero(tempValue1) ? 100.0m * (prevGain / tempValue1) : 0.0m;
            }
            else
            {
                while (today < startIdx)
                {
                    decimal tempValue1 = inReal[today];
                    decimal tempValue2 = tempValue1 - prevValue;
                    prevValue = tempValue1;

                    prevLoss *= optInTimePeriod - 1;
                    prevGain *= optInTimePeriod - 1;
                    if (tempValue2 < 0.0m)
                    {
                        prevLoss -= tempValue2;
                    }
                    else
                    {
                        prevGain += tempValue2;
                    }

                    prevLoss /= optInTimePeriod;
                    prevGain /= optInTimePeriod;

                    today++;
                }
            }

            while (today <= endIdx)
            {
                decimal tempValue1 = inReal[today++];
                decimal tempValue2 = tempValue1 - prevValue;
                prevValue = tempValue1;

                prevLoss *= optInTimePeriod - 1;
                prevGain *= optInTimePeriod - 1;
                if (tempValue2 < 0.0m)
                {
                    prevLoss -= tempValue2;
                }
                else
                {
                    prevGain += tempValue2;
                }

                prevLoss /= optInTimePeriod;
                prevGain /= optInTimePeriod;
                tempValue1 = prevGain + prevLoss;
                outReal[outIdx++] = !HighPerf.Lib.IsZero(tempValue1) ? 100.0m * (prevGain / tempValue1) : 0.0m;
            }

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int RsiLookback(int optInTimePeriod = 14)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            int retValue = optInTimePeriod + (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.Rsi];
            if (HighPerf.Lib.Globals.Compatibility == Compatibility.Metastock)
            {
                retValue--;
            }

            return retValue;
        }
    }
}
