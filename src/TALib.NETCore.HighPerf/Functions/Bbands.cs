using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Bbands(
            ref Span<double> input,
            ref Span<double> output,
            int inputSize,
            out int outputSize,
            //ref Span<double> inReal,
            //int startIdx,
            //int endIdx,
            //ref Span<double> outRealUpperBand,
            //ref Span<double> outRealMiddleBand,
            //ref Span<double> outRealLowerBand,
            //out int outBegIdx,
            //out int outNbElement,
            MAType optInMAType = MAType.Sma,
            int optInTimePeriod = 5,
            double optInNbDevUp = 2.0,
            double optInNbDevDn = 2.0)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                outputSize = 0;
                return RetCode.BadParam;
            }

            var startIdx = 0;
            var endIdx = inputSize - 1;

            var outRealUpperBand = output.Series(inputSize, 0);
            var outRealMiddleBand = output.Series(inputSize, 1);
            var outRealLowerBand = output.Series(inputSize, 2);

            //Span<double> tempBuffer1;
            //Span<double> tempBuffer2;
            //if (inReal == outRealUpperBand)
            //{
            //    tempBuffer1 = outRealMiddleBand;
            //    tempBuffer2 = outRealLowerBand;
            //}
            //else if (inReal == outRealLowerBand)
            //{
            //    tempBuffer1 = outRealMiddleBand;
            //    tempBuffer2 = outRealUpperBand;
            //}
            //else if (inReal == outRealMiddleBand)
            //{
            //    tempBuffer1 = outRealLowerBand;
            //    tempBuffer2 = outRealUpperBand;
            //}
            //else
            //{
            //    tempBuffer1 = outRealMiddleBand;
            //    tempBuffer2 = outRealUpperBand;
            //}

            var tempBuffer1 = outRealMiddleBand;
            var tempBuffer2 = outRealUpperBand;

            //if (tempBuffer1 == inReal || tempBuffer2 == inReal)
            //{
            //    return RetCode.BadParam;
            //}

            RetCode retCode = Ma(ref input, startIdx, endIdx, ref tempBuffer1, out var outBegIdx, out var outNbElement, optInMAType, optInTimePeriod);
            if (retCode != RetCode.Success || outNbElement == 0)
            {
                outputSize = 0;
                return retCode;
            }

            if (optInMAType == MAType.Sma)
            {
                INT_StdDevUsingPrecalcMA(ref input, ref tempBuffer1, outBegIdx, outNbElement, ref tempBuffer2, optInTimePeriod);
            }
            else
            {
                retCode = StdDev(ref input, outBegIdx, endIdx, ref tempBuffer2, out outBegIdx, out outNbElement, optInTimePeriod);
                if (retCode != RetCode.Success)
                {
                    outputSize = 0;
                    return retCode;
                }
            }

            if (tempBuffer1 != outRealMiddleBand)
            {
                BufferHelpers.Copy(ref tempBuffer1, 0, ref outRealMiddleBand, 0, outNbElement);
            }

            double tempReal;
            double tempReal2;
            if (optInNbDevUp.Equals(optInNbDevDn))
            {
                if (optInNbDevUp.Equals(1.0))
                {
                    for (var i = 0; i < outNbElement; i++)
                    {
                        tempReal = tempBuffer2[i];
                        tempReal2 = outRealMiddleBand[i];
                        outRealUpperBand[i] = tempReal2 + tempReal;
                        outRealLowerBand[i] = tempReal2 - tempReal;
                    }
                }
                else
                {
                    for (var i = 0; i < outNbElement; i++)
                    {
                        tempReal = tempBuffer2[i] * optInNbDevUp;
                        tempReal2 = outRealMiddleBand[i];
                        outRealUpperBand[i] = tempReal2 + tempReal;
                        outRealLowerBand[i] = tempReal2 - tempReal;
                    }
                }
            }
            else if (optInNbDevUp.Equals(1.0))
            {
                for (var i = 0; i < outNbElement; i++)
                {
                    tempReal = tempBuffer2[i];
                    tempReal2 = outRealMiddleBand[i];
                    outRealUpperBand[i] = tempReal2 + tempReal;
                    outRealLowerBand[i] = tempReal2 - tempReal * optInNbDevDn;
                }
            }
            else if (optInNbDevDn.Equals(1.0))
            {
                for (var i = 0; i < outNbElement; i++)
                {
                    tempReal = tempBuffer2[i];
                    tempReal2 = outRealMiddleBand[i];
                    outRealLowerBand[i] = tempReal2 - tempReal;
                    outRealUpperBand[i] = tempReal2 + tempReal * optInNbDevUp;
                }
            }
            else
            {
                for (var i = 0; i < outNbElement; i++)
                {
                    tempReal = tempBuffer2[i];
                    tempReal2 = outRealMiddleBand[i];
                    outRealUpperBand[i] = tempReal2 + tempReal * optInNbDevUp;
                    outRealLowerBand[i] = tempReal2 - tempReal * optInNbDevDn;
                }
            }

            outputSize = outNbElement;
            return RetCode.Success;
        }

        public static int BbandsLookback(MAType optInMAType = MAType.Sma, int optInTimePeriod = 5)
        {
            if (optInTimePeriod < 2 || optInTimePeriod > 100000)
            {
                return -1;
            }

            return MaLookback(optInMAType, optInTimePeriod);
        }
    }
}
