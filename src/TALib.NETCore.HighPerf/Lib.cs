using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        private static readonly GlobalsType Globals = new GlobalsType();

        static Lib()
        {
            RestoreCandleDefaultSettings(CandleSettingType.AllCandleSettings);
        }

        public static Span<decimal> Series(this Span<decimal> item, int size, int dimension, int subSetSize = -1)
        {
            if (subSetSize > size)
            {
                throw new ApplicationException("Subset size is greater than size.");
            }

            if (subSetSize < 0)
            {
                subSetSize = size;
            }
            return item.Slice(size * dimension, subSetSize);
        }

        public static Compatibility GetCompatibility() => Globals.Compatibility;

        public static RetCode SetCompatibility(Compatibility value)
        {
            Globals.Compatibility = value;

            return RetCode.Success;
        }

        public static long GetUnstablePeriod(FuncUnstId id) => id >= FuncUnstId.FuncUnstAll ? 0 : Globals.UnstablePeriod[(int) id];

        public static RetCode SetUnstablePeriod(FuncUnstId id, long unstablePeriod)
        {
            if (id > FuncUnstId.FuncUnstAll)
            {
                return RetCode.BadParam;
            }

            if (id != FuncUnstId.FuncUnstAll)
            {
                Globals.UnstablePeriod[(int) id] = unstablePeriod;
            }
            else
            {
                Array.Fill(Globals.UnstablePeriod, unstablePeriod);
            }

            return RetCode.Success;
        }

        private static RetCode RestoreCandleDefaultSettings(CandleSettingType settingType)
        {
            switch (settingType)
            {
                case CandleSettingType.BodyLong:
                    SetCandleSettings(CandleSettingType.BodyLong, RangeType.RealBody, 10, 1.0m);
                    break;
                case CandleSettingType.BodyVeryLong:
                    SetCandleSettings(CandleSettingType.BodyVeryLong, RangeType.RealBody, 10, 3.0m);
                    break;
                case CandleSettingType.BodyShort:
                    SetCandleSettings(CandleSettingType.BodyShort, RangeType.RealBody, 10, 1.0m);
                    break;
                case CandleSettingType.BodyDoji:
                    SetCandleSettings(CandleSettingType.BodyDoji, RangeType.HighLow, 10, 0.1m);
                    break;
                case CandleSettingType.ShadowLong:
                    SetCandleSettings(CandleSettingType.ShadowLong, RangeType.RealBody, 0, 1.0m);
                    break;
                case CandleSettingType.ShadowVeryLong:
                    SetCandleSettings(CandleSettingType.ShadowVeryLong, RangeType.RealBody, 0, 2.0m);
                    break;
                case CandleSettingType.ShadowShort:
                    SetCandleSettings(CandleSettingType.ShadowShort, RangeType.Shadows, 10, 1.0m);
                    break;
                case CandleSettingType.ShadowVeryShort:
                    SetCandleSettings(CandleSettingType.ShadowVeryShort, RangeType.HighLow, 10, 0.1m);
                    break;
                case CandleSettingType.Near:
                    SetCandleSettings(CandleSettingType.Near, RangeType.HighLow, 5, 0.2m);
                    break;
                case CandleSettingType.Far:
                    SetCandleSettings(CandleSettingType.Far, RangeType.HighLow, 5, 0.6m);
                    break;
                case CandleSettingType.Equal:
                    SetCandleSettings(CandleSettingType.Equal, RangeType.HighLow, 5, 0.05m);
                    break;
                case CandleSettingType.AllCandleSettings:
                    SetCandleSettings(CandleSettingType.BodyLong, RangeType.RealBody, 10, 1.0m);
                    SetCandleSettings(CandleSettingType.BodyVeryLong, RangeType.RealBody, 10, 3.0m);
                    SetCandleSettings(CandleSettingType.BodyShort, RangeType.RealBody, 10, 1.0m);
                    SetCandleSettings(CandleSettingType.BodyDoji, RangeType.HighLow, 10, 0.1m);
                    SetCandleSettings(CandleSettingType.ShadowLong, RangeType.RealBody, 0, 1.0m);
                    SetCandleSettings(CandleSettingType.ShadowVeryLong, RangeType.RealBody, 0, 2.0m);
                    SetCandleSettings(CandleSettingType.ShadowShort, RangeType.Shadows, 10, 1.0m);
                    SetCandleSettings(CandleSettingType.ShadowVeryShort, RangeType.HighLow, 10, 0.1m);
                    SetCandleSettings(CandleSettingType.Near, RangeType.HighLow, 5, 0.2m);
                    SetCandleSettings(CandleSettingType.Far, RangeType.HighLow, 5, 0.6m);
                    SetCandleSettings(CandleSettingType.Equal, RangeType.HighLow, 5, 0.05m);
                    break;
            }

            return RetCode.Success;
        }

        private static RetCode SetCandleSettings(CandleSettingType settingType, RangeType rangeType, int avgPeriod, decimal factor)
        {
            if (settingType >= CandleSettingType.AllCandleSettings)
            {
                return RetCode.BadParam;
            }

            Globals.CandleSettings[(int) settingType].SettingType = settingType;
            Globals.CandleSettings[(int) settingType].RangeType = rangeType;
            Globals.CandleSettings[(int) settingType].AvgPeriod = avgPeriod;
            Globals.CandleSettings[(int) settingType].Factor = factor;

            return RetCode.Success;
        }

        private static RetCode INT_EMA(
            ref Span<decimal> inReal,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
            out int outBegIdx,
            out int outNbElement,
            int optInTimePeriod,
            decimal optInK1)
        {
            outBegIdx = outNbElement = 0;

            int lookbackTotal = EmaLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            outBegIdx = startIdx;

            int today;
            decimal prevMA;
            if (Globals.Compatibility == Compatibility.Default)
            {
                today = startIdx - lookbackTotal;
                int i = optInTimePeriod;
                decimal tempReal = default;
                while (i-- > 0)
                {
                    tempReal += inReal[today++];
                }

                prevMA = tempReal / optInTimePeriod;
            }
            else
            {
                prevMA = inReal[0];
                today = 1;
            }

            while (today <= startIdx)
            {
                prevMA = (inReal[today++] - prevMA) * optInK1 + prevMA;
            }

            outReal[0] = prevMA;
            int outIdx = 1;
            while (today <= endIdx)
            {
                prevMA = (inReal[today++] - prevMA) * optInK1 + prevMA;
                outReal[outIdx++] = prevMA;
            }

            outNbElement = outIdx;

            return RetCode.Success;
        }

        private static RetCode INT_MACD(
            ref Span<decimal> input,
            ref Span<decimal> output,
            int inputSize,
            out int outputSize,
            int optInFastPeriod,
            int optInSlowPeriod,
            int optInSignalPeriod)
        {
            if (optInSlowPeriod < optInFastPeriod)
            {
                (optInSlowPeriod, optInFastPeriod) = (optInFastPeriod, optInSlowPeriod);
            }

            var startIdx = 0;
            var endIdx = inputSize - 1;

            decimal k1;
            decimal k2;
            if (optInSlowPeriod != 0)
            {
                k1 = 2.0m / (optInSlowPeriod + 1);
            }
            else
            {
                optInSlowPeriod = 26;
                k1 = 0.075m;
            }

            if (optInFastPeriod != 0)
            {
                k2 = 2.0m / (optInFastPeriod + 1);
            }
            else
            {
                optInFastPeriod = 12;
                k2 = 0.15m;
            }

            var lookbackSignal = EmaLookback(optInSignalPeriod);
            var lookbackTotal = MacdLookback(optInFastPeriod, optInSlowPeriod, optInSignalPeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                outputSize = 0;
                return RetCode.Success;
            }

            var tempInteger = endIdx - startIdx + 1 + lookbackSignal;
            var fastEMABuffer = BufferHelpers.New(tempInteger);
            var slowEMABuffer = BufferHelpers.New(tempInteger);

            tempInteger = startIdx - lookbackSignal;
            RetCode retCode = INT_EMA(
                ref input,
                tempInteger,
                endIdx,
                ref slowEMABuffer,
                out var outBegIdx1,
                out var outNbElement1,
                optInSlowPeriod,
                k1);

            if (retCode != RetCode.Success)
            {
                outputSize = 0;
                return retCode;
            }

            retCode = INT_EMA(
                ref input,
                tempInteger,
                endIdx,
                ref fastEMABuffer,
                out var outBegIdx2,
                out var outNbElement2,
                optInFastPeriod,
                k2);

            if (retCode != RetCode.Success)
            {
                outputSize = 0;
                return retCode;
            }

            if (outBegIdx1 != tempInteger ||
                outBegIdx2 != tempInteger ||
                outNbElement1 != outNbElement2 ||
                outNbElement1 != endIdx - startIdx + 1 + lookbackSignal)
            {
                outputSize = 0;
                return RetCode.InternalError;
            }

            for (var i = 0; i < outNbElement1; i++)
            {
                fastEMABuffer[i] -= slowEMABuffer[i];
            }

            outputSize = endIdx - startIdx + 1;
            var outMacd = output.Series(outputSize, 0);
            var outMacdSignal = output.Series(outputSize, 1);
            var outMacdHist = output.Series(outputSize, 2);

            BufferHelpers.Copy(ref fastEMABuffer, lookbackSignal, ref outMacd, 0, outputSize);

            retCode = INT_EMA(
                ref fastEMABuffer,
                0,
                outNbElement1 - 1,
                ref outMacdSignal,
                out _,
                out outNbElement2,
                optInSignalPeriod,
                2.0m / (optInSignalPeriod + 1));

            if (retCode != RetCode.Success)
            {
                return retCode;
            }

            for (var i = 0; i < outNbElement2; i++)
            {
                outMacdHist[i] = outMacd[i] - outMacdSignal[i];
            }

            output = output.Slice(0, 3 * outputSize);
            return RetCode.Success;
        }
        
        private static RetCode INT_PO(
            ref Span<decimal> inReal,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
            out int outBegIdx,
            out int outNbElement,
            int optInFastPeriod,
            int optInSlowPeriod,
            MAType optInMethod,
            ref Span<decimal> tempBuffer,
            bool doPercentageOutput)
        {
            outBegIdx = outNbElement = 0;

            if (optInSlowPeriod < optInFastPeriod)
            {
                (optInSlowPeriod, optInFastPeriod) = (optInFastPeriod, optInSlowPeriod);
            }

            RetCode retCode = Ma(ref inReal, startIdx, endIdx, ref tempBuffer, out var outBegIdx2, out _, optInMethod, optInFastPeriod);
            if (retCode != RetCode.Success)
            {
                return retCode;
            }

            retCode = Ma(ref inReal, startIdx, endIdx, ref outReal, out var outBegIdx1, out var outNbElement1, optInMethod, optInSlowPeriod);
            if (retCode != RetCode.Success)
            {
                return retCode;
            }

            for (int i = 0, j = outBegIdx1 - outBegIdx2; i < outNbElement1; i++, j++)
            {
                if (doPercentageOutput)
                {
                    decimal tempReal = outReal[i];
                    outReal[i] = !Lib.IsZero(tempReal) ? (tempBuffer[j] - tempReal) / tempReal * 100.0m : 0.0m;
                }
                else
                {
                    outReal[i] = tempBuffer[j] - outReal[i];
                }
            }

            outBegIdx = outBegIdx1;
            outNbElement = outNbElement1;

            return retCode;
        }

        private static RetCode INT_SMA(
            ref Span<decimal> inReal,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
            out int outBegIdx,
            out int outNbElement,
            int optInTimePeriod)
        {
            outBegIdx = outNbElement = 0;

            int lookbackTotal = Lib.SmaLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            decimal periodTotal = default;
            int trailingIdx = startIdx - lookbackTotal;
            int i = trailingIdx;
            if (optInTimePeriod > 1)
            {
                while (i < startIdx)
                {
                    periodTotal += inReal[i++];
                }
            }

            int outIdx = default;
            do
            {
                periodTotal += inReal[i++];
                decimal tempReal = periodTotal;
                periodTotal -= inReal[trailingIdx++];
                outReal[outIdx++] = tempReal / optInTimePeriod;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        private static void INT_StdDevUsingPrecalcMA(
            ref Span<decimal> inReal,
            ref Span<decimal> inMovAvg,
            int inMovAvgBegIdx,
            int inMovAvgNbElement,
            ref Span<decimal> outReal,
            int optInTimePeriod)
        {
            int startSum = inMovAvgBegIdx + 1 - optInTimePeriod;
            int endSum = inMovAvgBegIdx;
            decimal periodTotal2 = default;
            for (int outIdx = startSum; outIdx < endSum; outIdx++)
            {
                decimal tempReal = inReal[outIdx];
                tempReal *= tempReal;
                periodTotal2 += tempReal;
            }

            for (var outIdx = 0; outIdx < inMovAvgNbElement; outIdx++, startSum++, endSum++)
            {
                decimal tempReal = inReal[endSum];
                tempReal *= tempReal;
                periodTotal2 += tempReal;
                decimal meanValue2 = periodTotal2 / optInTimePeriod;

                tempReal = inReal[startSum];
                tempReal *= tempReal;
                periodTotal2 -= tempReal;

                tempReal = inMovAvg[outIdx];
                tempReal *= tempReal;
                meanValue2 -= tempReal;

                outReal[outIdx] = !Lib.IsZeroOrNeg(meanValue2) ? (decimal) Math.Sqrt((double)meanValue2) : 0.0m;
            }
        }

        private static RetCode INT_VAR(
            ref Span<decimal> inReal,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
            out int outBegIdx,
            out int outNbElement,
            int optInTimePeriod)
        {
            outBegIdx = outNbElement = 0;

            int lookbackTotal = Lib.VarLookback(optInTimePeriod);
            if (startIdx < lookbackTotal)
            {
                startIdx = lookbackTotal;
            }

            if (startIdx > endIdx)
            {
                return RetCode.Success;
            }

            decimal periodTotal1 = default;
            decimal periodTotal2 = default;
            int trailingIdx = startIdx - lookbackTotal;
            int i = trailingIdx;
            if (optInTimePeriod > 1)
            {
                while (i < startIdx)
                {
                    decimal tempReal = inReal[i++];
                    periodTotal1 += tempReal;
                    tempReal *= tempReal;
                    periodTotal2 += tempReal;
                }
            }

            int outIdx = default;
            do
            {
                decimal tempReal = inReal[i++];
                periodTotal1 += tempReal;
                tempReal *= tempReal;
                periodTotal2 += tempReal;
                decimal meanValue1 = periodTotal1 / optInTimePeriod;
                decimal meanValue2 = periodTotal2 / optInTimePeriod;
                tempReal = inReal[trailingIdx++];
                periodTotal1 -= tempReal;
                tempReal *= tempReal;
                periodTotal2 -= tempReal;
                outReal[outIdx++] = meanValue2 - meanValue1 * meanValue1;
            } while (i <= endIdx);

            outBegIdx = startIdx;
            outNbElement = outIdx;

            return RetCode.Success;
        }

        private class CandleSetting
        {
            internal int AvgPeriod;
            internal decimal Factor;
            internal RangeType RangeType;
            internal CandleSettingType SettingType;
        }
        
        private class GlobalsType
        {
            internal Compatibility Compatibility = Compatibility.Default;
            internal readonly CandleSetting[] CandleSettings;
            internal readonly long[] UnstablePeriod;

            internal GlobalsType()
            {
                CandleSettings = new CandleSetting[(int) CandleSettingType.AllCandleSettings];
                UnstablePeriod = new long[(int) FuncUnstId.FuncUnstAll];

                Array.Fill(CandleSettings, new CandleSetting());
            }
        }
    }
}
