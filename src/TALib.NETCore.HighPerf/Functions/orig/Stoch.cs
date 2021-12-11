using System;

namespace TALib.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Stoch(
            ref Span<double> inHigh,
            ref Span<double> inLow,
            ref Span<double> inClose,
            int startIdx,
            int endIdx,
            ref Span<double> outSlowK,
            ref Span<double> outSlowD,
            out int outBegIdx,
            out int outNbElement,
            MAType optInSlowKMAType = MAType.Sma,
            MAType optInSlowDMAType = MAType.Sma,
            int optInFastKPeriod = 5,
            int optInSlowKPeriod = 3,
            int optInSlowDPeriod = 3)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inHigh == null || inLow == null || inClose == null || outSlowK == null || outSlowD == null || optInFastKPeriod < 1 ||
                optInFastKPeriod > 100000 || optInSlowKPeriod < 1 || optInSlowKPeriod > 100000 || optInSlowDPeriod < 1 ||
                optInSlowDPeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackK = optInFastKPeriod - 1;
            int lookbackDSlow = MaLookback(optInSlowDMAType, optInSlowDPeriod);
            int lookbackTotal = StochLookback(optInSlowKMAType, optInSlowDMAType, optInFastKPeriod, optInSlowKPeriod, optInSlowDPeriod);
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
            if (outSlowK == inHigh || outSlowK == inLow || outSlowK == inClose)
            {
                tempBuffer = outSlowK;
            }
            else if (outSlowD == inHigh || outSlowD == inLow || outSlowD == inClose)
            {
                tempBuffer = outSlowD;
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

            RetCode retCode = Ma(ref tempBuffer, 0, outIdx - 1, ref tempBuffer, out _, out outNbElement, optInSlowKMAType, optInSlowKPeriod);
            if (retCode != RetCode.Success || outNbElement == 0)
            {
                return retCode;
            }

            retCode = Ma(ref tempBuffer, 0, outNbElement - 1, ref outSlowD, out _, out outNbElement, optInSlowDMAType, optInSlowDPeriod);
            BufferHelpers.Copy(ref tempBuffer, lookbackDSlow, ref outSlowK, 0, outNbElement);
            if (retCode != RetCode.Success)
            {
                outNbElement = 0;

                return retCode;
            }

            outBegIdx = startIdx;

            return RetCode.Success;
        }

        public static int StochLookback(MAType optInSlowKMAType = MAType.Sma, MAType optInSlowDMAType = MAType.Sma,
            int optInFastKPeriod = 5, int optInSlowKPeriod = 3, int optInSlowDPeriod = 3)
        {
            if (optInFastKPeriod < 1 || optInFastKPeriod > 100000 || optInSlowKPeriod < 1 || optInSlowKPeriod > 100000 ||
                optInSlowDPeriod < 1 || optInSlowDPeriod > 100000)
            {
                return -1;
            }

            int retValue = optInFastKPeriod - 1;
            retValue += MaLookback(optInSlowKMAType, optInSlowKPeriod);
            retValue += MaLookback(optInSlowDMAType, optInSlowDPeriod);

            return retValue;
        }
    }
}
