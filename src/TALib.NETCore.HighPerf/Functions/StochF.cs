using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode StochF(
            ref Span<double> inHigh,
            ref Span<double> inLow,
            ref Span<double> inClose,
            int startIdx,
            int endIdx,
            ref Span<double> outFastK,
            ref Span<double> outFastD,
            out int outBegIdx,
            out int outNbElement,
            MAType optInFastDMAType = MAType.Sma,
            int optInFastKPeriod = 5,
            int optInFastDPeriod = 3)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inHigh == null || inLow == null || inClose == null || outFastK == null || outFastD == null || optInFastKPeriod < 1 ||
                optInFastKPeriod > 100000 || optInFastDPeriod < 1 || optInFastDPeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackK = optInFastKPeriod - 1;
            int lookbackFastD = MaLookback(optInFastDMAType, optInFastDPeriod);
            int lookbackTotal = StochFLookback(optInFastDMAType, optInFastKPeriod, optInFastDPeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            int outIdx = default;
            int trailingIdx = startIdx - lookbackTotal;
            int today = trailingIdx + lookbackK;
            int highestIdx = -1;
            int lowestIdx = highestIdx;
            double highest, lowest;
            double diff = highest = lowest = default;
            Span<double> tempBuffer;
            if (outFastK == inHigh || outFastK == inLow || outFastK == inClose)
            {
                tempBuffer = outFastK;
            }
            else if (outFastD == inHigh || outFastD == inLow || outFastD == inClose)
            {
                tempBuffer = outFastD;
            }
            else
            {
                tempBuffer = BufferHelpers.New(endIdx - today + 1);
            }

            while (today <= endIdx)
            {
                double tmp = inLow[today];
                if (lowestIdx < trailingIdx)
                {
                    lowestIdx = trailingIdx;
                    lowest = inLow[lowestIdx];
                    int i = lowestIdx;
                    while (++i <= today)
                    {
                        tmp = inLow[i];
                        if (tmp < lowest)
                        {
                            lowestIdx = i;
                            lowest = tmp;
                        }
                    }

                    diff = (highest - lowest) / 100.0;
                }
                else if (tmp <= lowest)
                {
                    lowestIdx = today;
                    lowest = tmp;
                    diff = (highest - lowest) / 100.0;
                }

                tmp = inHigh[today];
                if (highestIdx < trailingIdx)
                {
                    highestIdx = trailingIdx;
                    highest = inHigh[highestIdx];
                    int i = highestIdx;
                    while (++i <= today)
                    {
                        tmp = inHigh[i];
                        if (tmp > highest)
                        {
                            highestIdx = i;
                            highest = tmp;
                        }
                    }

                    diff = (highest - lowest) / 100.0;
                }
                else if (tmp >= highest)
                {
                    highestIdx = today;
                    highest = tmp;
                    diff = (highest - lowest) / 100.0;
                }

                tempBuffer[outIdx++] = !diff.Equals(0.0) ? (inClose[today] - lowest) / diff : 0.0;

                trailingIdx++;
                today++;
            }

            RetCode retCode = Ma(ref tempBuffer, 0, outIdx - 1, ref outFastD, out _, out outNbElement, optInFastDMAType, optInFastDPeriod);
            if (retCode != RetCode.Success || outNbElement == 0)
            {
                return retCode;
            }

            BufferHelpers.Copy(ref tempBuffer, lookbackFastD, ref outFastK, 0, outNbElement);
            outBegIdx = startIdx;

            return RetCode.Success;
        }

        public static int StochFLookback(MAType optInFastDMAType = MAType.Sma, int optInFastKPeriod = 5, int optInFastDPeriod = 3)
        {
            if (optInFastKPeriod < 1 || optInFastKPeriod > 100000 || optInFastDPeriod < 1 || optInFastDPeriod > 100000)
            {
                return -1;
            }

            int retValue = optInFastKPeriod - 1;

            return retValue + MaLookback(optInFastDMAType, optInFastDPeriod);
        }
    }
}
