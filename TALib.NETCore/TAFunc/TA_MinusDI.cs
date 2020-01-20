using System;

namespace TALib
{
    public partial class Core
    {
        public static RetCode MinusDI(int startIdx, int endIdx, double[] inHigh, double[] inLow, double[] inClose, ref int outBegIdx,
            ref int outNBElement, double[] outReal, int optInTimePeriod = 14)
        {
            double tempReal;
            int today;
            double tempReal2;
            double prevLow;
            double prevHigh;
            double diffM;
            double prevClose;
            double diffP;
            int lookbackTotal;
            if (startIdx < 0)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeEndIndex;
            }

            if (inHigh == null || inLow == null || inClose == null)
            {
                return RetCode.BadParam;
            }

            if (optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            if (outReal == null)
            {
                return RetCode.BadParam;
            }

            if (optInTimePeriod > 1)
            {
                lookbackTotal = optInTimePeriod + (int) Globals.UnstablePeriod[(int) FuncUnstId.MinusDI];
            }
            else
            {
                lookbackTotal = 1;
            }

            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                outBegIdx = 0;
                outNBElement = 0;
                return RetCode.Success;
            }

            int outIdx = default;
            if (optInTimePeriod > 1)
            {
                today = startIdx;
                outBegIdx = today;
                double prevMinusDM = default;
                double prevTR = default;
                today = startIdx - lookbackTotal;
                prevHigh = inHigh[today];
                prevLow = inLow[today];
                prevClose = inClose[today];
                int i = optInTimePeriod - 1;
                while (true)
                {
                    i--;
                    if (i <= 0)
                    {
                        i = (int) Globals.UnstablePeriod[(int) FuncUnstId.MinusDI] + 1;
                        while (true)
                        {
                            i--;
                            if (i == 0)
                            {
                                break;
                            }

                            today++;
                            tempReal = inHigh[today];
                            diffP = tempReal - prevHigh;
                            prevHigh = tempReal;
                            tempReal = inLow[today];
                            diffM = prevLow - tempReal;
                            prevLow = tempReal;
                            if (diffM > 0.0 && diffP < diffM)
                            {
                                prevMinusDM = prevMinusDM - prevMinusDM / optInTimePeriod + diffM;
                            }
                            else
                            {
                                prevMinusDM -= prevMinusDM / optInTimePeriod;
                            }

                            tempReal = prevHigh - prevLow;
                            tempReal2 = Math.Abs(prevHigh - prevClose);
                            if (tempReal2 > tempReal)
                            {
                                tempReal = tempReal2;
                            }

                            tempReal2 = Math.Abs(prevLow - prevClose);
                            if (tempReal2 > tempReal)
                            {
                                tempReal = tempReal2;
                            }

                            prevTR = prevTR - prevTR / optInTimePeriod + tempReal;
                            prevClose = inClose[today];
                        }

                        if (-1E-08 >= prevTR || prevTR >= 1E-08)
                        {
                            outReal[0] = 100.0 * (prevMinusDM / prevTR);
                        }
                        else
                        {
                            outReal[0] = 0.0;
                        }

                        outIdx = 1;
                        while (today < endIdx)
                        {
                            today++;
                            tempReal = inHigh[today];
                            diffP = tempReal - prevHigh;
                            prevHigh = tempReal;
                            tempReal = inLow[today];
                            diffM = prevLow - tempReal;
                            prevLow = tempReal;
                            if (diffM > 0.0 && diffP < diffM)
                            {
                                prevMinusDM = prevMinusDM - prevMinusDM / optInTimePeriod + diffM;
                            }
                            else
                            {
                                prevMinusDM -= prevMinusDM / optInTimePeriod;
                            }

                            tempReal = prevHigh - prevLow;
                            tempReal2 = Math.Abs(prevHigh - prevClose);
                            if (tempReal2 > tempReal)
                            {
                                tempReal = tempReal2;
                            }

                            tempReal2 = Math.Abs(prevLow - prevClose);
                            if (tempReal2 > tempReal)
                            {
                                tempReal = tempReal2;
                            }

                            prevTR = prevTR - prevTR / optInTimePeriod + tempReal;
                            prevClose = inClose[today];
                            if (-1E-08 >= prevTR || prevTR >= 1E-08)
                            {
                                outReal[outIdx] = 100.0 * (prevMinusDM / prevTR);
                                outIdx++;
                            }
                            else
                            {
                                outReal[outIdx] = 0.0;
                                outIdx++;
                            }
                        }

                        outNBElement = outIdx;
                        return RetCode.Success;
                    }

                    today++;
                    tempReal = inHigh[today];
                    diffP = tempReal - prevHigh;
                    prevHigh = tempReal;
                    tempReal = inLow[today];
                    diffM = prevLow - tempReal;
                    prevLow = tempReal;
                    if (diffM > 0.0 && diffP < diffM)
                    {
                        prevMinusDM += diffM;
                    }

                    tempReal = prevHigh - prevLow;
                    tempReal2 = Math.Abs(prevHigh - prevClose);
                    if (tempReal2 > tempReal)
                    {
                        tempReal = tempReal2;
                    }

                    tempReal2 = Math.Abs(prevLow - prevClose);
                    if (tempReal2 > tempReal)
                    {
                        tempReal = tempReal2;
                    }

                    prevTR += tempReal;
                    prevClose = inClose[today];
                }
            }

            outBegIdx = startIdx;
            today = startIdx - 1;
            prevHigh = inHigh[today];
            prevLow = inLow[today];
            prevClose = inClose[today];
            while (true)
            {
                if (today >= endIdx)
                {
                    break;
                }

                today++;
                tempReal = inHigh[today];
                diffP = tempReal - prevHigh;
                prevHigh = tempReal;
                tempReal = inLow[today];
                diffM = prevLow - tempReal;
                prevLow = tempReal;
                if (diffM > 0.0 && diffP < diffM)
                {
                    tempReal = prevHigh - prevLow;
                    tempReal2 = Math.Abs(prevHigh - prevClose);
                    if (tempReal2 > tempReal)
                    {
                        tempReal = tempReal2;
                    }

                    tempReal2 = Math.Abs(prevLow - prevClose);
                    if (tempReal2 > tempReal)
                    {
                        tempReal = tempReal2;
                    }

                    if (-1E-08 < tempReal && tempReal < 1E-08)
                    {
                        outReal[outIdx] = 0.0;
                        outIdx++;
                    }
                    else
                    {
                        outReal[outIdx] = diffM / tempReal;
                        outIdx++;
                    }
                }
                else
                {
                    outReal[outIdx] = 0.0;
                    outIdx++;
                }

                prevClose = inClose[today];
            }

            outNBElement = outIdx;
            return RetCode.Success;
        }

        public static RetCode MinusDI(int startIdx, int endIdx, decimal[] inHigh, decimal[] inLow, decimal[] inClose, ref int outBegIdx,
            ref int outNBElement, decimal[] outReal, int optInTimePeriod = 14)
        {
            decimal tempReal;
            int today;
            decimal tempReal2;
            decimal prevLow;
            decimal prevHigh;
            decimal diffM;
            decimal prevClose;
            decimal diffP;
            int lookbackTotal;
            if (startIdx < 0)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeEndIndex;
            }

            if (inHigh == null || inLow == null || inClose == null)
            {
                return RetCode.BadParam;
            }

            if (optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            if (outReal == null)
            {
                return RetCode.BadParam;
            }

            if (optInTimePeriod > 1)
            {
                lookbackTotal = optInTimePeriod + (int) Globals.UnstablePeriod[(int) FuncUnstId.MinusDI];
            }
            else
            {
                lookbackTotal = 1;
            }

            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                outBegIdx = 0;
                outNBElement = 0;
                return RetCode.Success;
            }

            int outIdx = default;
            if (optInTimePeriod > 1)
            {
                today = startIdx;
                outBegIdx = today;
                decimal prevMinusDM = default;
                decimal prevTR = default;
                today = startIdx - lookbackTotal;
                prevHigh = inHigh[today];
                prevLow = inLow[today];
                prevClose = inClose[today];
                int i = optInTimePeriod - 1;
                while (true)
                {
                    i--;
                    if (i <= 0)
                    {
                        i = (int) Globals.UnstablePeriod[(int) FuncUnstId.MinusDI] + 1;
                        while (true)
                        {
                            i--;
                            if (i == 0)
                            {
                                break;
                            }

                            today++;
                            tempReal = inHigh[today];
                            diffP = tempReal - prevHigh;
                            prevHigh = tempReal;
                            tempReal = inLow[today];
                            diffM = prevLow - tempReal;
                            prevLow = tempReal;
                            if (diffM > Decimal.Zero && diffP < diffM)
                            {
                                prevMinusDM = prevMinusDM - prevMinusDM / optInTimePeriod + diffM;
                            }
                            else
                            {
                                prevMinusDM -= prevMinusDM / optInTimePeriod;
                            }

                            tempReal = prevHigh - prevLow;
                            tempReal2 = Math.Abs(prevHigh - prevClose);
                            if (tempReal2 > tempReal)
                            {
                                tempReal = tempReal2;
                            }

                            tempReal2 = Math.Abs(prevLow - prevClose);
                            if (tempReal2 > tempReal)
                            {
                                tempReal = tempReal2;
                            }

                            prevTR = prevTR - prevTR / optInTimePeriod + tempReal;
                            prevClose = inClose[today];
                        }

                        if (-1E-08m >= prevTR || prevTR >= 1E-08m)
                        {
                            outReal[0] = 100m * (prevMinusDM / prevTR);
                        }
                        else
                        {
                            outReal[0] = Decimal.Zero;
                        }

                        outIdx = 1;
                        while (today < endIdx)
                        {
                            today++;
                            tempReal = inHigh[today];
                            diffP = tempReal - prevHigh;
                            prevHigh = tempReal;
                            tempReal = inLow[today];
                            diffM = prevLow - tempReal;
                            prevLow = tempReal;
                            if (diffM > Decimal.Zero && diffP < diffM)
                            {
                                prevMinusDM = prevMinusDM - prevMinusDM / optInTimePeriod + diffM;
                            }
                            else
                            {
                                prevMinusDM -= prevMinusDM / optInTimePeriod;
                            }

                            tempReal = prevHigh - prevLow;
                            tempReal2 = Math.Abs(prevHigh - prevClose);
                            if (tempReal2 > tempReal)
                            {
                                tempReal = tempReal2;
                            }

                            tempReal2 = Math.Abs(prevLow - prevClose);
                            if (tempReal2 > tempReal)
                            {
                                tempReal = tempReal2;
                            }

                            prevTR = prevTR - prevTR / optInTimePeriod + tempReal;
                            prevClose = inClose[today];
                            if (-1E-08m >= prevTR || prevTR >= 1E-08m)
                            {
                                outReal[outIdx] = 100m * (prevMinusDM / prevTR);
                                outIdx++;
                            }
                            else
                            {
                                outReal[outIdx] = Decimal.Zero;
                                outIdx++;
                            }
                        }

                        outNBElement = outIdx;
                        return RetCode.Success;
                    }

                    today++;
                    tempReal = inHigh[today];
                    diffP = tempReal - prevHigh;
                    prevHigh = tempReal;
                    tempReal = inLow[today];
                    diffM = prevLow - tempReal;
                    prevLow = tempReal;
                    if (diffM > Decimal.Zero && diffP < diffM)
                    {
                        prevMinusDM += diffM;
                    }

                    tempReal = prevHigh - prevLow;
                    tempReal2 = Math.Abs(prevHigh - prevClose);
                    if (tempReal2 > tempReal)
                    {
                        tempReal = tempReal2;
                    }

                    tempReal2 = Math.Abs(prevLow - prevClose);
                    if (tempReal2 > tempReal)
                    {
                        tempReal = tempReal2;
                    }

                    prevTR += tempReal;
                    prevClose = inClose[today];
                }
            }

            outBegIdx = startIdx;
            today = startIdx - 1;
            prevHigh = inHigh[today];
            prevLow = inLow[today];
            prevClose = inClose[today];
            while (true)
            {
                if (today >= endIdx)
                {
                    break;
                }

                today++;
                tempReal = inHigh[today];
                diffP = tempReal - prevHigh;
                prevHigh = tempReal;
                tempReal = inLow[today];
                diffM = prevLow - tempReal;
                prevLow = tempReal;
                if (diffM > Decimal.Zero && diffP < diffM)
                {
                    tempReal = prevHigh - prevLow;
                    tempReal2 = Math.Abs(prevHigh - prevClose);
                    if (tempReal2 > tempReal)
                    {
                        tempReal = tempReal2;
                    }

                    tempReal2 = Math.Abs(prevLow - prevClose);
                    if (tempReal2 > tempReal)
                    {
                        tempReal = tempReal2;
                    }

                    if (-1E-08m < tempReal && tempReal < 1E-08m)
                    {
                        outReal[outIdx] = Decimal.Zero;
                        outIdx++;
                    }
                    else
                    {
                        outReal[outIdx] = diffM / tempReal;
                        outIdx++;
                    }
                }
                else
                {
                    outReal[outIdx] = Decimal.Zero;
                    outIdx++;
                }

                prevClose = inClose[today];
            }

            outNBElement = outIdx;
            return RetCode.Success;
        }

        public static int MinusDILookback(int optInTimePeriod = 14)
        {
            if (optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return optInTimePeriod > 1 ? optInTimePeriod + (int) Globals.UnstablePeriod[(int) FuncUnstId.MinusDI] : 1;
        }
    }
}