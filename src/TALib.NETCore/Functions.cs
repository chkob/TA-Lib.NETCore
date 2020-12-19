using System;
using System.Collections;
using System.Collections.Generic;

namespace TALib
{
    public class Functions : IEnumerable<Function>
    {
        private const string RealType = "Real";
        private const string IntegerType = "Integer";

        internal static readonly IDictionary<string, Function> FunctionsDefinition = new Dictionary<string, Function>
        {
            { "Accbands", new Function("Accbands", "Acceleration Bands", "Overlap Studies", "High|Low|Close", "Time Period", "Real Upper Band|Real Middle Band|Real Lower Band") },
            { "Acos", new Function("Acos", "Vector Trigonometric ACos", "Math Transform", RealType, String.Empty, RealType) },
            { "Ad", new Function("Ad", "Chaikin A/D Line", "Volume Indicators", "High|Low|Close|Volume", String.Empty, RealType) },
            { "Add", new Function("Add", "Vector Arithmetic Add", "Math Operators", RealType + "|" + RealType, String.Empty, RealType) },
            { "AdOsc", new Function("AdOsc", "Chaikin A/D Oscillator", "Volume Indicators", "High|Low|Close|Volume", "Fast Period|Slow Period", RealType) },
            { "Adx", new Function("Adx", "Average Directional Movement Index", "Momentum Indicators", "High|Low|Close", "Time Period", RealType) },
            { "Adxr", new Function("Adxr", "Average Directional Movement Index Rating", "Momentum Indicators", "High|Low|Close", "Time Period", RealType) },
            { "Apo", new Function("Apo", "Absolute Price Oscillator", "Momentum Indicators", RealType, "MA Type|Fast Period|Slow Period", RealType) },
            { "Aroon", new Function("Aroon", "Aroon", "Momentum Indicators", "High|Low", "Time Period", "Aroon Down|Aroon Up") },
            { "AroonOsc", new Function("AroonOsc", "Aroon Oscillator", "Momentum Indicators", "High|Low", "Time Period", RealType) },
            { "Asin", new Function("Asin", "Vector Trigonometric ASin", "Math Transform", RealType, String.Empty, RealType) },
            { "Atan", new Function("Atan", "Vector Trigonometric ATan", "Math Transform", RealType, String.Empty, RealType) },
            { "Atr", new Function("Atr", "Average True Range", "Volatility Indicators", "High|Low|Close", "Time Period", RealType) },
            { "AvgDev", new Function("AvgDev", "Average Deviation", "Price Transform", RealType, "Time Period", RealType) },
            { "AvgPrice", new Function("AvgPrice", "Average Price", "Price Transform", "Open|High|Low|Close", String.Empty, RealType) },
            { "Bbands", new Function("Bbands", "Bollinger Bands", "Overlap Studies", RealType, "MA Type|Time Period|Nb Dev Up|Nb Dev Dn", "Real Upper Band|Real Middle Band|Real Lower Band") },
            { "Beta", new Function("Beta", "Beta", "Statistic Functions", RealType + "|" + RealType, "Time Period", RealType) },
            { "Bop", new Function("Bop", "Balance of Power", "Momentum Indicators", "Open|High|Low|Close", String.Empty, RealType) },
            { "Cci", new Function("Cci", "Commodity Channel Index", "Momentum Indicators", "High|Low|Close", "Time Period", RealType) },
            { "Ceil", new Function("Ceil", "Vector Ceil", "Math Transform", RealType, String.Empty, RealType) },
            { "Cmo", new Function("Cmo", "Chande Momentum Oscillator", "Momentum Indicators", RealType, "Time Period", RealType) },
            { "Correl", new Function("Correl", "Pearson's Correlation Coefficient (r)", "Statistic Functions", RealType + "|" + RealType, "Time Period", RealType) },
            { "Cos", new Function("Cos", "Vector Trigonometric Cos", "Math Transform", RealType, String.Empty, RealType) },
            { "Cosh", new Function("Cosh", "Vector Trigonometric Cosh", "Math Transform", RealType, String.Empty, RealType) },
            { "Dema", new Function("Dema", "Double Exponential Moving Average", "Overlap Studies", RealType, "Time Period", RealType) },
            { "Div", new Function("Div", "Vector Arithmetic Div", "Math Operators", RealType + "|" + RealType, String.Empty, RealType) },
            { "Dx", new Function("Dx", "Directional Movement Index", "Momentum Indicators", "High|Low|Close", "Time Period", RealType) },
            { "Ema", new Function("Ema", "Exponential Moving Average", "Overlap Studies", RealType, "Time Period", RealType) },
            { "Exp", new Function("Exp", "Vector Arithmetic Exp", "Math Transform", RealType, String.Empty, RealType) },
            { "Floor", new Function("Floor", "Vector Floor", "Math Transform", RealType, String.Empty, RealType) },
            { "HtDcPeriod", new Function("HtDcPeriod", "Hilbert Transform - Dominant Cycle Period", "Cycle Indicators", RealType, String.Empty, RealType) },
            { "HtDcPhase", new Function("HtDcPhase", "Hilbert Transform - Dominant Cycle Phase", "Cycle Indicators", RealType, String.Empty, RealType) },
            { "HtPhasor", new Function("HtPhasor", "Hilbert Transform - Phasor Components", "Cycle Indicators", RealType, String.Empty, "In Phase|Quadrature") },
            { "HtSine", new Function("HtSine", "Hilbert Transform - SineWave", "Cycle Indicators", RealType, String.Empty, "Sine|Lead Sine") },
            { "HtTrendline", new Function("HtTrendline", "Hilbert Transform - Instantaneous Trendline", "Overlap Studies", RealType, String.Empty, RealType) },
            { "HtTrendMode", new Function("HtTrendMode", "Hilbert Transform - Trend vs Cycle Mode", "Cycle Indicators", RealType, String.Empty, IntegerType) },
            { "Kama", new Function("Kama", "Kaufman Adaptive Moving Average", "Overlap Studies", RealType, "Time Period", RealType) },
            { "LinearReg", new Function("LinearReg", "Linear Regression", "Statistic Functions", RealType, "Time Period", RealType) },
            { "LinearRegAngle", new Function("LinearRegAngle", "Linear Regression Angle", "Statistic Functions", RealType, "Time Period", RealType) },
            { "LinearRegIntercept", new Function("LinearRegIntercept", "Linear Regression Intercept", "Statistic Functions", RealType, "Time Period", RealType) },
            { "LinearRegSlope", new Function("LinearRegSlope", "Linear Regression Slope", "Statistic Functions", RealType, "Time Period", RealType) },
            { "Ln", new Function("Ln", "Vector Log Natural", "Math Transform", RealType, String.Empty, RealType) },
            { "Log10", new Function("Log10", "Vector Log10", "Math Transform", RealType, String.Empty, RealType) },
            { "Ma", new Function("Ma", "Moving Average", "Overlap Studies", RealType, "MA Type|Time Period", RealType) },
            { "Macd", new Function("Macd", "Moving Average Convergence/Divergence", "Momentum Indicators", RealType, "Fast Period|Slow Period|Signal Period", "Macd|Macd Signal|Macd Hist") },
            { "MacdExt", new Function("MacdExt", "MACD with controllable MA type", "Momentum Indicators", RealType, "Fast MA Type|Slow MA Type|Signal MA Type|Fast Period|Slow Period|Signal Period", "Macd|Macd Signal|Macd Hist") },
            { "MacdFix", new Function("MacdFix", "Moving Average Convergence/Divergence Fix 12/26", "Momentum Indicators", RealType, "Signal Period", "Macd|Macd Signal|Macd Hist") },
            { "Mama", new Function("Mama", "MESA Adaptive Moving Average", "Overlap Studies", RealType, "Fast Limit|Slow Limit", "Mama|Fama") },
            { "Mavp", new Function("Mavp", "Moving average with variable period", "Overlap Studies", RealType + "|Periods", "Min Period|Max Period", RealType) },
            { "Max", new Function("Max", "Highest value over a specified period", "Math Operators", RealType, "Time Period", RealType) },
            { "MaxIndex", new Function("MaxIndex", "Index of highest value over a specified period", "Math Operators", RealType, "Time Period", IntegerType) },
            { "MedPrice", new Function("MedPrice", "Median Price", "Price Transform", "High|Low", String.Empty, RealType) },
            { "Mfi", new Function("Mfi", "Money Flow Index", "Momentum Indicators", "High|Low|Close|Volume", "Time Period", RealType) },
            { "MidPoint", new Function("MidPoint", "MidPoint over period", "Overlap Studies", RealType, "Time Period", RealType) },
            { "MidPrice", new Function("MidPrice", "Midpoint Price over period", "Overlap Studies", "High|Low", "Time Period", RealType) },
            { "Min", new Function("Min", "Lowest value over a specified period", "Math Operators", RealType, "Time Period", RealType) },
            { "MinIndex", new Function("MinIndex", "Index of lowest value over a specified period", "Math Operators", RealType, "Time Period", IntegerType) },
            { "MinMax", new Function("MinMax", "Lowest and highest values over a specified period", "Math Operators", RealType, "Time Period", "Min|Max") },
            { "MinMaxIndex", new Function("MinMaxIndex", "Indexes of lowest and highest values over a specified period", "Math Operators", RealType, "Time Period", "Min Idx|Max Idx") },
            { "MinusDI", new Function("MinusDI", "Minus Directional Indicator", "Momentum Indicators", "High|Low|Close", "Time Period", RealType) },
            { "MinusDM", new Function("MinusDM", "Minus Directional Movement", "Momentum Indicators", "High|Low", "Time Period", RealType) },
            { "Mom", new Function("Mom", "Momentum", "Momentum Indicators", RealType, "Time Period", RealType) },
            { "Mult", new Function("Mult", "Vector Arithmetic Mult", "Math Operators", RealType + "|" + RealType, String.Empty, RealType) },
            { "Natr", new Function("Natr", "Normalized Average True Range", "Volatility Indicators", "High|Low|Close", "Time Period", RealType) },
            { "Obv", new Function("Obv", "On Balance Volume", "Volume Indicators", RealType + "|Volume", String.Empty, RealType) },
            { "PlusDI", new Function("PlusDI", "Plus Directional Indicator", "Momentum Indicators", "High|Low|Close", "Time Period", RealType) },
            { "PlusDM", new Function("PlusDM", "Plus Directional Movement", "Momentum Indicators", "High|Low", "Time Period", RealType) },
            { "Ppo", new Function("Ppo", "Percentage Price Oscillator", "Momentum Indicators", RealType, "MA Type|Fast Period|Slow Period", RealType) },
            { "Roc", new Function("Roc", "Rate of change : ((price/prevPrice)-1)*100", "Momentum Indicators", RealType, "Time Period", RealType) },
            { "RocP", new Function("RocP", "Rate of change Percentage: (price-prevPrice)/prevPrice", "Momentum Indicators", RealType, "Time Period", RealType) },
            { "RocR", new Function("RocR", "Rate of change ratio: (price/prevPrice)", "Momentum Indicators", RealType, "Time Period", RealType) },
            { "RocR100", new Function("RocR100", "Rate of change ratio 100 scale: (price/prevPrice)*100", "Momentum Indicators", RealType, "Time Period", RealType) },
            { "Rsi", new Function("Rsi", "Relative Strength Index", "Momentum Indicators", RealType, "Time Period", RealType) },
            { "Sar", new Function("Sar", "Parabolic SAR", "Overlap Studies", "High|Low", "Acceleration|Maximum", RealType) },
            { "SarExt", new Function("SarExt", "Parabolic SAR - Extended", "Overlap Studies", "High|Low", "Start Value|Offset On Reverse|Acceleration Init Long|Acceleration Long|Acceleration Max Long|Acceleration Init Short|Acceleration Short|Acceleration Max Short", RealType) },
            { "Sin", new Function("Sin", "Vector Trigonometric Sin", "Math Transform", RealType, String.Empty, RealType) },
            { "Sinh", new Function("Sinh", "Vector Trigonometric Sinh", "Math Transform", RealType, String.Empty, RealType) },
            { "Sma", new Function("Sma", "Simple Moving Average", "Overlap Studies", RealType, "Time Period", RealType) },
            { "Sqrt", new Function("Sqrt", "Vector Square Root", "Math Transform", RealType, String.Empty, RealType) },
            { "StdDev", new Function("StdDev", "Standard Deviation", "Statistic Functions", RealType, "Time Period|Nb Dev", RealType) },
            { "Stoch", new Function("Stoch", "Stochastic", "Momentum Indicators", "High|Low|Close", "Slow K MA Type|Slow D MA Type|Fast K Period|Slow K Period|Slow D Period", "Slow K|Slow D") },
            { "StochF", new Function("StochF", "Stochastic Fast", "Momentum Indicators", "High|Low|Close", "Fast D MA Type|Fast K Period|Fast D Period", "Fast K|Fast D") },
            { "StochRsi", new Function("StochRsi", "Stochastic Relative Strength Index", "Momentum Indicators", RealType, "Fast D MA Type|Time Period|Fast K Period|Fast D Period", "Fast K|Fast D") },
            { "Sub", new Function("Sub", "Vector Arithmetic Subtraction", "Math Operators", RealType + "|" + RealType, String.Empty, RealType) },
            { "Sum", new Function("Sum", "Summation", "Math Operators", RealType, "Time Period", RealType) },
            { "T3", new Function("T3", "Triple Exponential Moving Average (T3)", "Overlap Studies", RealType, "Time Period|V Factor", RealType) },
            { "Tan", new Function("Tan", "Vector Trigonometric Tan", "Math Transform", RealType, String.Empty, RealType) },
            { "Tanh", new Function("Tanh", "Vector Trigonometric Tanh", "Math Transform", RealType, String.Empty, RealType) },
            { "Tema", new Function("Tema", "Triple Exponential Moving Average", "Overlap Studies", RealType, "Time Period", RealType) },
            { "TRange", new Function("TRange", "True Range", "Volatility Indicators", "High|Low|Close", String.Empty, RealType) },
            { "Trima", new Function("Trima", "Triangular Moving Average", "Overlap Studies", RealType, "Time Period", RealType) },
            { "Trix", new Function("Trix", "1-day Rate-Of-Change (ROC) of a Triple Smooth EMA", "Momentum Indicators", RealType, "Time Period", RealType) },
            { "Tsf", new Function("Tsf", "Time Series Forecast", "Statistic Functions", RealType, "Time Period", RealType) },
            { "TypPrice", new Function("TypPrice", "Typical Price", "Price Transform", "High|Low|Close", String.Empty, RealType) },
            { "UltOsc", new Function("UltOsc", "Ultimate Oscillator", "Momentum Indicators", "High|Low|Close", "Time Period 1|Time Period 2|Time Period 3", RealType) },
            { "Var", new Function("Var", "Variance", "Statistic Functions", RealType, "Time Period", RealType) },
            { "WclPrice", new Function("WclPrice", "Weighted Close Price", "Price Transform", "High|Low|Close", String.Empty, RealType) },
            { "WillR", new Function("WillR", "Williams' %R", "Momentum Indicators", "High|Low|Close", "Time Period", RealType) },
            { "Wma", new Function("Wma", "Weighted Moving Average", "Overlap Studies", RealType, "Time Period", RealType) },

            { "Cdl2Crows", new Function("Cdl2Crows", "Two Crows", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "Cdl3BlackCrows", new Function("Cdl3BlackCrows", "Three Black Crows", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "Cdl3Inside", new Function("Cdl3Inside", "Three Inside Up/Down", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "Cdl3LineStrike", new Function("Cdl3LineStrike", "Three-Line Strike", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "Cdl3Outside", new Function("Cdl3Outside", "Three Outside Up/Down", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "Cdl3StarsInSouth", new Function("Cdl3StarsInSouth", "Three Stars In The South", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "Cdl3WhiteSoldiers", new Function("Cdl3WhiteSoldiers", "Three Advancing White Soldiers", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlAbandonedBaby", new Function("CdlAbandonedBaby", "Abandoned Baby", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlAdvanceBlock", new Function("CdlAdvanceBlock", "Advance Block", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlBeltHold", new Function("CdlBeltHold", "Belt-hold", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlBreakaway", new Function("CdlBreakaway", "Breakaway", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlClosingMarubozu", new Function("CdlClosingMarubozu", "Closing Marubozu", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlConcealBabysWall", new Function("CdlConcealBabysWall", "Concealing Baby Swallow", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlCounterAttack", new Function("CdlCounterAttack", "Counterattack", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlDarkCloudCover", new Function("CdlDarkCloudCover", "Dark Cloud Cover", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlDoji", new Function("CdlDoji", "Doji", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlDojiStar", new Function("CdlDojiStar", "Doji Star", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlDragonflyDoji", new Function("CdlDragonflyDoji", "Dragonfly Doji", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlEngulfing", new Function("CdlEngulfing", "Engulfing Pattern", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlEveningDojiStar", new Function("CdlEveningDojiStar", "Evening Doji Star", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlEveningStar", new Function("CdlEveningStar", "Evening Star", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlGapSideSideWhite", new Function("CdlGapSideSideWhite", "Up/Down-gap side-by-side white lines", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlGravestoneDoji", new Function("CdlGravestoneDoji", "Gravestone Doji", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlHammer", new Function("CdlHammer", "Hammer", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlHangingMan", new Function("CdlHangingMan", "Hanging Man", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlHarami", new Function("CdlHarami", "Harami Pattern", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlHaramiCross", new Function("CdlHaramiCross", "Harami Cross Pattern", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlHighWave", new Function("CdlHighWave", "High-Wave Candle", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlHikkake", new Function("CdlHikkake", "Hikkake Pattern", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlHikkakeMod", new Function("CdlHikkakeMod", "Modified Hikkake Pattern", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlHomingPigeon", new Function("CdlHomingPigeon", "Homing Pigeon", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlIdentical3Crows", new Function("CdlIdentical3Crows", "Identical Three Crows", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlInNeck", new Function("CdlInNeck", "In-Neck Pattern", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlInvertedHammer", new Function("CdlInvertedHammer", "Inverted Hammer", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlKicking", new Function("CdlKicking", "Kicking", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlKickingByLength", new Function("CdlKickingByLength", "Kicking - bull/bear determined by the longer marubozu", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlLadderBottom", new Function("CdlLadderBottom", "Ladder Bottom", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlLongLeggedDoji", new Function("CdlLongLeggedDoji", "Long Legged Doji", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlLongLine", new Function("CdlLongLine", "Long Line Candle", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlMarubozu", new Function("CdlMarubozu", "Marubozu", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlMatchingLow", new Function("CdlMatchingLow", "Matching Low", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlMatHold", new Function("CdlMatHold", "Mat Hold", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlMorningDojiStar", new Function("CdlMorningDojiStar", "Morning Doji Star", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlMorningStar", new Function("CdlMorningStar", "Morning Star", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlOnNeck", new Function("CdlOnNeck", "On-Neck Pattern", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlPiercing", new Function("CdlPiercing", "Piercing Pattern", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlRickshawMan", new Function("CdlRickshawMan", "Rickshaw Man", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlRiseFall3Methods", new Function("CdlRiseFall3Methods", "Rising/Falling Three Methods", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlSeparatingLines", new Function("CdlSeparatingLines", "Separating Lines", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlShootingStar", new Function("CdlShootingStar", "Shooting Star", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlShortLine", new Function("CdlShortLine", "Short Line Candle", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlSpinningTop", new Function("CdlSpinningTop", "Spinning Top", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlStalledPattern", new Function("CdlStalledPattern", "Stalled Pattern", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlStickSandwich", new Function("CdlStickSandwich", "Stick Sandwich", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlTakuri", new Function("CdlTakuri", "Takuri (Dragonfly Doji with very long lower shadow)", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlTasukiGap", new Function("CdlTasukiGap", "Tasuki Gap", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlThrusting", new Function("CdlThrusting", "Thrusting Pattern", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlTristar", new Function("CdlTristar", "Tristar Pattern", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlUnique3River", new Function("CdlUnique3River", "Unique 3 River", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlUpsideGap2Crows", new Function("CdlUpsideGap2Crows", "Upside Gap Two Crows", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) },
            { "CdlXSideGap3Methods", new Function("CdlXSideGap3Methods", "Upside/Downside Gap Three Methods", "Pattern Recognition", "Open|High|Low|Close", String.Empty, IntegerType) }
        };

        public static Function Find(string name) =>
            FunctionsDefinition.TryGetValue(name, out var function)
                ? function
                : throw new ArgumentException("Function not found", name);

        public IEnumerator<Function> GetEnumerator() => FunctionsDefinition.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}