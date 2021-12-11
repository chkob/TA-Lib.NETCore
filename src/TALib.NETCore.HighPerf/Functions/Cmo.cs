using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Cmo(ref Span<double> inReal, int startIdx, int endIdx, ref Span<double> outReal, out int outBegIdx, out int outNbElement,
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

            int lookbackTotal = CmoLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            double prevLoss;
            double prevGain;
            double tempValue1;
            double tempValue2;
            int outIdx = default;
            int today = startIdx - lookbackTotal;
            double prevValue = inReal[today];
            if (HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.Cmo] == 0 && HighPerf.Lib.Globals.Compatibility == Compatibility.Metastock)
            {
                double savePrevValue = prevValue;
                prevGain = 0.0;
                prevLoss = 0.0;
                for (int i = optInTimePeriod; i > 0; i--)
                {
                    tempValue1 = inReal[today++];
                    tempValue2 = tempValue1 - prevValue;
                    prevValue = tempValue1;
                    if (tempValue2 < 0.0)
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
                double tempValue3 = tempValue2 - tempValue1;
                double tempValue4 = tempValue1 + tempValue2;
                outReal[outIdx++] = !HighPerf.Lib.IsZero(tempValue4) ? 100.0 * (tempValue3 / tempValue4) : 0.0;

                if (today > endIdx)
                {
                    outBegIdx = startIdx;
                    outNbElement = outIdx;

                    return RetCode.Success;
                }

                today -= optInTimePeriod;
                prevValue = savePrevValue;
            }

            prevGain = 0.0;
            prevLoss = 0.0;
            today++;
            for (int i = optInTimePeriod; i > 0; i--)
            {
                tempValue1 = inReal[today++];
                tempValue2 = tempValue1 - prevValue;
                prevValue = tempValue1;
                if (tempValue2 < 0.0)
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
                tempValue1 = prevGain + prevLoss;
                outReal[outIdx++] = !HighPerf.Lib.IsZero(tempValue1) ? 100.0 * ((prevGain - prevLoss) / tempValue1) : 0.0;
            }
            else
            {
                while (today < startIdx)
                {
                    tempValue1 = inReal[today];
                    tempValue2 = tempValue1 - prevValue;
                    prevValue = tempValue1;

                    prevLoss *= optInTimePeriod - 1;
                    prevGain *= optInTimePeriod - 1;
                    if (tempValue2 < 0.0)
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
                tempValue1 = inReal[today++];
                tempValue2 = tempValue1 - prevValue;
                prevValue = tempValue1;

                prevLoss *= optInTimePeriod - 1;
                prevGain *= optInTimePeriod - 1;
                if (tempValue2 < 0.0)
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
                outReal[outIdx++] = !HighPerf.Lib.IsZero(tempValue1) ? 100.0 * ((prevGain - prevLoss) / tempValue1) : 0.0;
            }

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int CmoLookback(int optInTimePeriod = 14)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            int retValue = optInTimePeriod + (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.Cmo];
            if (HighPerf.Lib.Globals.Compatibility == Compatibility.Metastock)
            {
                retValue--;
            }

            return retValue;
        }
    }
}
