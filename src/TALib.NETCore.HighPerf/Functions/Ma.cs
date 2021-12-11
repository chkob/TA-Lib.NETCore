using System;

namespace TALib.NETCore.HighPerf
{
    public static partial class Lib
    {
        public static RetCode Ma(
            ref Span<decimal> inReal,
            int startIdx,
            int endIdx,
            ref Span<decimal> outReal,
            out int outBegIdx,
            out int outNbElement,
            MAType optInMAType = MAType.Sma,
            int optInTimePeriod = 30)
        {
            outBegIdx = outNbElement = 0;

            if (startIdx < 0 || endIdx < 0 || endIdx < startIdx)
            {
                return RetCode.OutOfRangeStartIndex;
            }

            if (inReal == null || outReal == null || optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return RetCode.BadParam;
            }

            if (optInTimePeriod == 1)
            {
                var nbElement = endIdx - startIdx + 1;
                outNbElement = nbElement;
                for (int todayIdx = startIdx, outIdx = 0; outIdx < nbElement; outIdx++, todayIdx++)
                {
                    outReal[outIdx] = inReal[todayIdx];
                }

                outBegIdx = startIdx;
                return RetCode.Success;
            }

            switch (optInMAType)
            {
                case MAType.Sma:
                    return Sma(ref inReal, startIdx, endIdx, ref outReal, out outBegIdx, out outNbElement, optInTimePeriod);
                case MAType.Ema:
                    return Ema(ref inReal, startIdx, endIdx, ref outReal, out outBegIdx, out outNbElement, optInTimePeriod);
                case MAType.Wma:
                    return Wma(ref inReal, startIdx, endIdx, ref outReal, out outBegIdx, out outNbElement, optInTimePeriod);
                case MAType.Dema:
                    return Dema(ref inReal, startIdx, endIdx, ref outReal, out outBegIdx, out outNbElement, optInTimePeriod);
                case MAType.Tema:
                    return Tema(ref inReal, startIdx, endIdx, ref outReal, out outBegIdx, out outNbElement, optInTimePeriod);
                case MAType.Trima:
                    return Trima(ref inReal, startIdx, endIdx, ref outReal, out outBegIdx, out outNbElement, optInTimePeriod);
                case MAType.Kama:
                    return Kama(ref inReal, startIdx, endIdx, ref outReal, out outBegIdx, out outNbElement, optInTimePeriod);
                case MAType.Mama:
                    var dummyBuffer = BufferHelpers.New(endIdx - startIdx + 1);
                    return Mama(ref inReal, ref outReal, endIdx + 1, out outNbElement);
                case MAType.T3:
                    return T3(ref inReal, startIdx, endIdx, ref outReal, out outBegIdx, out outNbElement, optInTimePeriod);
                default:
                    return RetCode.BadParam;
            }
        }

        public static int MaLookback(MAType optInMAType = MAType.Sma, int optInTimePeriod = 30)
        {
            if (optInTimePeriod < 1 || optInTimePeriod > 100000)
            {
                return -1;
            }

            if (optInTimePeriod == 1)
            {
                return 0;
            }

            return optInMAType switch
            {
                MAType.Sma => SmaLookback(optInTimePeriod),
                MAType.Ema => EmaLookback(optInTimePeriod),
                MAType.Wma => WmaLookback(optInTimePeriod),
                MAType.Dema => DemaLookback(optInTimePeriod),
                MAType.Tema => TemaLookback(optInTimePeriod),
                MAType.Trima => TrimaLookback(optInTimePeriod),
                MAType.Kama => KamaLookback(optInTimePeriod),
                MAType.Mama => MamaLookback(),
                MAType.T3 => T3Lookback(optInTimePeriod),
                _ => 0
            };
        }
    }
}
