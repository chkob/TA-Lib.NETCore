using System;
using System.Collections.Generic;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        private const decimal Epsilon = 0.00000000000001m;

        private static decimal RealBody(
            ref Span<decimal> close,
            ref Span<decimal> open,
            int idx)
            => Math.Abs(close[idx] - open[idx]);

        private static decimal UpperShadow(
            ref Span<decimal> high,
            ref Span<decimal> close,
            ref Span<decimal> open, int idx)
            => high[idx] - (close[idx] >= open[idx] ? close[idx] : open[idx]);

        private static decimal LowerShadow(
            ref Span<decimal> close,
            ref Span<decimal> open,
            ref Span<decimal> low,
            int idx)
            => (close[idx] >= open[idx] ? open[idx] : close[idx]) - low[idx];

        private static decimal HighLowRange(
            ref Span<decimal> high,
            ref Span<decimal> low,
            int idx)
            => high[idx] - low[idx];

        private static bool CandleColor(
            ref Span<decimal> close,
            ref Span<decimal> open,
            int idx)
            => close[idx] >= open[idx];

        private static RangeType CandleRangeType(
            CandleSettingType set)
            => Globals.CandleSettings[(int) set].RangeType;

        private static int CandleAvgPeriod(
            CandleSettingType set)
            => Globals.CandleSettings[(int) set].AvgPeriod;

        private static decimal CandleFactor(
            CandleSettingType set)
            => Globals.CandleSettings[(int) set].Factor;

        private static decimal CandleRange(
            ref Span<decimal> open,
            ref Span<decimal> high,
            ref Span<decimal> low,
            ref Span<decimal> close,
            CandleSettingType set, int idx) =>
            CandleRangeType(set) switch
            {
                RangeType.RealBody => RealBody(ref close, ref open, idx),
                RangeType.HighLow => HighLowRange(ref high, ref low, idx),
                RangeType.Shadows => UpperShadow(ref high, ref close, ref open, idx) + LowerShadow(ref close, ref open, ref low, idx),
                _ => 0
            };

        private static decimal CandleAverage(
            ref Span<decimal> open,
            ref Span<decimal> high,
            ref Span<decimal> low,
            ref Span<decimal> close,
            CandleSettingType set,
            decimal sum,
            int idx) =>
            CandleFactor(set) * (CandleAvgPeriod(set) != 0
                ? sum / CandleAvgPeriod(set)
                : CandleRange(ref open, ref high, ref low, ref close, set, idx)) / (CandleRangeType(set) == RangeType.Shadows ? 2.0m : 1.0m);

        private static bool RealBodyGapUp(
            ref Span<decimal> open,
            ref Span<decimal> close,
            int idx2,
            int idx1) => Math.Min(open[idx2], close[idx2]) > Math.Max(open[idx1], close[idx1]);

        private static bool RealBodyGapDown(
            ref Span<decimal> open,
            ref Span<decimal> close,
            int idx2,
            int idx1) => Math.Max(open[idx2], close[idx2]) < Math.Min(open[idx1], close[idx1]);

        private static bool CandleGapUp(
            ref Span<decimal> low,
            ref Span<decimal> high,
            int idx2,
            int idx1) => low[idx2] > high[idx1];

        private static bool CandleGapDown(
            ref Span<decimal> low,
            ref Span<decimal> high,
            int idx2,
            int idx1) => high[idx2] < low[idx1];

        private static bool IsZero(
            decimal v) => -Epsilon < v && v < Epsilon;
        
        private static bool IsZeroOrNeg(
            decimal v) => v < Epsilon;
        
        private static void TrueRange(
            decimal th,
            decimal tl,
            decimal yc,
            out decimal @out)
        {
            @out = th - tl;
            decimal tempdecimal = Math.Abs(th - yc);
            if (tempdecimal > @out)
            {
                @out = tempdecimal;
            }

            tempdecimal = Math.Abs(tl - yc);
            if (tempdecimal > @out)
            {
                @out = tempdecimal;
            }
        }

        private static void DoPriceWma(
            ref Span<decimal> real,
            ref int idx,
            ref decimal periodWMASub,
            ref decimal periodWMASum,
            ref decimal trailingWMAValue,
            decimal varNewPrice,
            out decimal varToStoreSmoothedValue)
        {
            periodWMASub += varNewPrice;
            periodWMASub -= trailingWMAValue;
            periodWMASum += varNewPrice * 4.0m;
            trailingWMAValue = real[idx++];
            varToStoreSmoothedValue = periodWMASum * 0.1m;
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
            IDictionary<string, decimal> variables,
            string varName,
            decimal input,
            string oddOrEvenId,
            int hilbertIdx,
            decimal adjustedPrevPeriod)
        {
            const decimal a = 0.0962m;
            const decimal b = 0.5769m;

            var hilbertTempdecimal = a * input;
            variables[varName] = -variables[$"{varName}{oddOrEvenId}{hilbertIdx}"];
            variables[$"{varName}{oddOrEvenId}{hilbertIdx}"] = hilbertTempdecimal;
            variables[varName] += hilbertTempdecimal;
            variables[varName] -= variables[$"prev{varName}{oddOrEvenId}"];
            variables[$"prev{varName}{oddOrEvenId}"] = b * variables[$"prev{varName}Input{oddOrEvenId}"];
            variables[varName] += variables[$"prev{varName}{oddOrEvenId}"];
            variables[$"prev{varName}Input{oddOrEvenId}"] = input;
            variables[varName] *= adjustedPrevPeriod;
        }

        private static void DoHilbertOdd(
            IDictionary<string, decimal> variables,
            string varName,
            decimal input,
            int hilbertIdx,
            decimal adjustedPrevPeriod) =>
            DoHilbertTransform(variables, varName, input, "Odd", hilbertIdx, adjustedPrevPeriod);

        private static void DoHilbertEven(
            IDictionary<string, decimal> variables,
            string varName,
            decimal input,
            int hilbertIdx,
            decimal adjustedPrevPeriod) =>
            DoHilbertTransform(variables, varName, input, "Even", hilbertIdx, adjustedPrevPeriod);

        private static void CalcTerms(
            ref Span<decimal> inLow,
            ref Span<decimal> inHigh,
            ref Span<decimal> inClose,
            int day,
            out decimal trueRange,
            out decimal closeMinusTrueLow)
        {
            var tempLT = inLow[day];
            var tempHT = inHigh[day];
            var tempCY = inClose[day - 1];
            var trueLow = Math.Min(tempLT, tempCY);
            closeMinusTrueLow = inClose[day] - trueLow;
            trueRange = tempHT - tempLT;
            var tempdecimal = Math.Abs(tempCY - tempHT);
            if (tempdecimal > trueRange)
            {
                trueRange = tempdecimal;
            }

            tempdecimal = Math.Abs(tempCY - tempLT);
            if (tempdecimal > trueRange)
            {
                trueRange = tempdecimal;
            }
        }
    }
}
