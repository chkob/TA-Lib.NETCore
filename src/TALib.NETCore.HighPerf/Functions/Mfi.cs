using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Mfi(
            ref Span<decimal> inHigh,
            ref Span<decimal> inLow,
            ref Span<decimal> inClose,
            ref Span<decimal> inVolume,
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

            if (inHigh == null || inLow == null || inClose == null || inVolume == null || outReal == null || optInTimePeriod < 2 ||
                optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            int lookbackTotal = MfiLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            int outIdx = default;

            var moneyFlow = new (decimal negative, decimal positive)[optInTimePeriod];

            int mflowIdx = default;
            var maxIdxMflow = optInTimePeriod - 1;

            int today = startIdx - lookbackTotal;
            decimal prevValue = (inHigh[today] + inLow[today] + inClose[today]) / 3.0m;

            decimal posSumMF = default;
            decimal negSumMF = default;
            today++;
            for (int i = optInTimePeriod; i > 0; i--)
            {
                decimal tempValue1 = (inHigh[today] + inLow[today] + inClose[today]) / 3.0m;
                decimal tempValue2 = tempValue1 - prevValue;
                prevValue = tempValue1;
                tempValue1 *= inVolume[today++];
                if (tempValue2 < 0.0m)
                {
                    moneyFlow[mflowIdx].negative = tempValue1;
                    negSumMF += tempValue1;
                    moneyFlow[mflowIdx].positive = 0.0m;
                }
                else if (tempValue2 > 0.0m)
                {
                    moneyFlow[mflowIdx].positive = tempValue1;
                    posSumMF += tempValue1;
                    moneyFlow[mflowIdx].negative = 0.0m;
                }
                else
                {
                    moneyFlow[mflowIdx].positive = 0.0m;
                    moneyFlow[mflowIdx].negative = 0.0m;
                }

                if (++mflowIdx > maxIdxMflow)
                {
                    mflowIdx = 0;
                }
            }

            if (today > startIdx)
            {
                decimal tempValue1 = posSumMF + negSumMF;
                outReal[outIdx++] = tempValue1 >= 1.0m ? 100.0m * (posSumMF / tempValue1) : 0.0m;
            }
            else
            {
                while (today < startIdx)
                {
                    posSumMF -= moneyFlow[mflowIdx].positive;
                    negSumMF -= moneyFlow[mflowIdx].negative;

                    decimal tempValue1 = (inHigh[today] + inLow[today] + inClose[today]) / 3.0m;
                    decimal tempValue2 = tempValue1 - prevValue;
                    prevValue = tempValue1;
                    tempValue1 *= inVolume[today++];
                    if (tempValue2 < 0.0m)
                    {
                        moneyFlow[mflowIdx].negative = tempValue1;
                        negSumMF += tempValue1;
                        moneyFlow[mflowIdx].positive = 0.0m;
                    }
                    else if (tempValue2 > 0.0m)
                    {
                        moneyFlow[mflowIdx].positive = tempValue1;
                        posSumMF += tempValue1;
                        moneyFlow[mflowIdx].negative = 0.0m;
                    }
                    else
                    {
                        moneyFlow[mflowIdx].positive = 0.0m;
                        moneyFlow[mflowIdx].negative = 0.0m;
                    }

                    if (++mflowIdx > maxIdxMflow)
                    {
                        mflowIdx = 0;
                    }
                }
            }

            while (today <= endIdx)
            {
                posSumMF -= moneyFlow[mflowIdx].positive;
                negSumMF -= moneyFlow[mflowIdx].negative;

                decimal tempValue1 = (inHigh[today] + inLow[today] + inClose[today]) / 3.0m;
                decimal tempValue2 = tempValue1 - prevValue;
                prevValue = tempValue1;
                tempValue1 *= inVolume[today++];
                if (tempValue2 < 0.0m)
                {
                    moneyFlow[mflowIdx].negative = tempValue1;
                    negSumMF += tempValue1;
                    moneyFlow[mflowIdx].positive = 0.0m;
                }
                else if (tempValue2 > 0.0m)
                {
                    moneyFlow[mflowIdx].positive = tempValue1;
                    posSumMF += tempValue1;
                    moneyFlow[mflowIdx].negative = 0.0m;
                }
                else
                {
                    moneyFlow[mflowIdx].positive = 0.0m;
                    moneyFlow[mflowIdx].negative = 0.0m;
                }

                tempValue1 = posSumMF + negSumMF;
                outReal[outIdx++] = tempValue1 >= 1.0m ? 100.0m * (posSumMF / tempValue1) : 0.0m;

                if (++mflowIdx > maxIdxMflow)
                {
                    mflowIdx = 0;
                }
            }

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        public static int MfiLookback(int optInTimePeriod = 14)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod + (int) HighPerf.Lib.Globals.UnstablePeriod[(int) FuncUnstId.Mfi];
        }
    }
}
