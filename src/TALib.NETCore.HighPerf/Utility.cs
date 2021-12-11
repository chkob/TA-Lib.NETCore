using System;
using System.Collections.Generic;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        private const double Epsilon = 0.00000000000001;

        private static double RealBody(
            ref Span<double> close,
            ref Span<double> open,
            int idx)
            => Math.Abs(close[idx] - open[idx]);

        private static double UpperShadow(
            ref Span<double> high,
            ref Span<double> close,
            ref Span<double> open, int idx)
            => high[idx] - (close[idx] >= open[idx] ? close[idx] : open[idx]);

        private static double LowerShadow(
            ref Span<double> close,
            ref Span<double> open,
            ref Span<double> low,
            int idx)
            => (close[idx] >= open[idx] ? open[idx] : close[idx]) - low[idx];

        private static double HighLowRange(
            ref Span<double> high,
            ref Span<double> low,
            int idx)
            => high[idx] - low[idx];

        private static bool CandleColor(
            ref Span<double> close,
            ref Span<double> open,
            int idx)
            => close[idx] >= open[idx];

        private static RangeType CandleRangeType(
            CandleSettingType set)
            => Globals.CandleSettings[(int) set].RangeType;

        private static int CandleAvgPeriod(
            CandleSettingType set)
            => Globals.CandleSettings[(int) set].AvgPeriod;

        private static double CandleFactor(
            CandleSettingType set)
            => Globals.CandleSettings[(int) set].Factor;

        private static double CandleRange(
            ref Span<double> open,
            ref Span<double> high,
            ref Span<double> low,
            ref Span<double> close,
            CandleSettingType set, int idx) =>
            CandleRangeType(set) switch
            {
                RangeType.RealBody => RealBody(ref close, ref open, idx),
                RangeType.HighLow => HighLowRange(ref high, ref low, idx),
                RangeType.Shadows => UpperShadow(ref high, ref close, ref open, idx) + LowerShadow(ref close, ref open, ref low, idx),
                _ => 0
            };

        private static double CandleAverage(
            ref Span<double> open,
            ref Span<double> high,
            ref Span<double> low,
            ref Span<double> close,
            CandleSettingType set,
            double sum,
            int idx) =>
            CandleFactor(set) * (CandleAvgPeriod(set) != 0
                ? sum / CandleAvgPeriod(set)
                : CandleRange(ref open, ref high, ref low, ref close, set, idx)) / (CandleRangeType(set) == RangeType.Shadows ? 2.0 : 1.0);

        private static bool RealBodyGapUp(
            ref Span<double> open,
            ref Span<double> close,
            int idx2,
            int idx1) => Math.Min(open[idx2], close[idx2]) > Math.Max(open[idx1], close[idx1]);

        private static bool RealBodyGapDown(
            ref Span<double> open,
            ref Span<double> close,
            int idx2,
            int idx1) => Math.Max(open[idx2], close[idx2]) < Math.Min(open[idx1], close[idx1]);

        private static bool CandleGapUp(
            ref Span<double> low,
            ref Span<double> high,
            int idx2,
            int idx1) => low[idx2] > high[idx1];

        private static bool CandleGapDown(
            ref Span<double> low,
            ref Span<double> high,
            int idx2,
            int idx1) => high[idx2] < low[idx1];

        private static bool IsZero(
            double v) => -Epsilon < v && v < Epsilon;
        
        private static bool IsZeroOrNeg(
            double v) => v < Epsilon;
        
        private static void TrueRange(
            double th,
            double tl,
            double yc,
            out double @out)
        {
            @out = th - tl;
            double tempDouble = Math.Abs(th - yc);
            if (tempDouble > @out)
            {
                @out = tempDouble;
            }

            tempDouble = Math.Abs(tl - yc);
            if (tempDouble > @out)
            {
                @out = tempDouble;
            }
        }

        private static void DoPriceWma(
            ref Span<double> real,
            ref int idx,
            ref double periodWMASub,
            ref double periodWMASum,
            ref double trailingWMAValue,
            double varNewPrice,
            out double varToStoreSmoothedValue)
        {
            periodWMASub += varNewPrice;
            periodWMASub -= trailingWMAValue;
            periodWMASum += varNewPrice * 4.0;
            trailingWMAValue = real[idx++];
            varToStoreSmoothedValue = periodWMASum * 0.1;
            periodWMASum -= periodWMASub;
        }

        private static IDictionary<string, T> InitHilbertVariables<T>() where T : struct, IComparable<T>
        {
            var variables = new Dictionary<string, T>(4 * 11);

            new List<string> { "detrender", "q1", "jI", "jQ" }.ForEach(varName =>
            {
                variables.Add($"{varName}Odd0", default);
                variables.Add($"{varName}Odd1", default);
                variables.Add($"{varName}Odd2", default);
                variables.Add($"{varName}Even0", default);
                variables.Add($"{varName}Even1", default);
                variables.Add($"{varName}Even2", default);
                variables.Add(varName, default);
                variables.Add($"prev{varName}Odd", default);
                variables.Add($"prev{varName}Even", default);
                variables.Add($"prev{varName}InputOdd", default);
                variables.Add($"prev{varName}InputEven", default);
            });

            return variables;
        }

        private static void DoHilbertTransform(
            IDictionary<string, double> variables,
            string varName,
            double input,
            string oddOrEvenId,
            int hilbertIdx,
            double adjustedPrevPeriod)
        {
            const double a = 0.0962;
            const double b = 0.5769;

            var hilbertTempDouble = a * input;
            variables[varName] = -variables[$"{varName}{oddOrEvenId}{hilbertIdx}"];
            variables[$"{varName}{oddOrEvenId}{hilbertIdx}"] = hilbertTempDouble;
            variables[varName] += hilbertTempDouble;
            variables[varName] -= variables[$"prev{varName}{oddOrEvenId}"];
            variables[$"prev{varName}{oddOrEvenId}"] = b * variables[$"prev{varName}Input{oddOrEvenId}"];
            variables[varName] += variables[$"prev{varName}{oddOrEvenId}"];
            variables[$"prev{varName}Input{oddOrEvenId}"] = input;
            variables[varName] *= adjustedPrevPeriod;
        }

        private static void DoHilbertOdd(
            IDictionary<string, double> variables,
            string varName,
            double input,
            int hilbertIdx,
            double adjustedPrevPeriod) =>
            DoHilbertTransform(variables, varName, input, "Odd", hilbertIdx, adjustedPrevPeriod);

        private static void DoHilbertEven(
            IDictionary<string, double> variables,
            string varName,
            double input,
            int hilbertIdx,
            double adjustedPrevPeriod) =>
            DoHilbertTransform(variables, varName, input, "Even", hilbertIdx, adjustedPrevPeriod);

        private static void CalcTerms(
            ref Span<double> inLow,
            ref Span<double> inHigh,
            ref Span<double> inClose,
            int day,
            out double trueRange,
            out double closeMinusTrueLow)
        {
            var tempLT = inLow[day];
            var tempHT = inHigh[day];
            var tempCY = inClose[day - 1];
            var trueLow = Math.Min(tempLT, tempCY);
            closeMinusTrueLow = inClose[day] - trueLow;
            trueRange = tempHT - tempLT;
            var tempDouble = Math.Abs(tempCY - tempHT);
            if (tempDouble > trueRange)
            {
                trueRange = tempDouble;
            }

            tempDouble = Math.Abs(tempCY - tempLT);
            if (tempDouble > trueRange)
            {
                trueRange = tempDouble;
            }
        }
    }
}
