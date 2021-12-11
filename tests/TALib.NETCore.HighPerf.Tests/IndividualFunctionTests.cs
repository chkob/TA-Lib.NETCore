using System;
using Xunit;

namespace TALib.NETCore.HighPerf.Tests
{
    public class IndividualFunctionTests
    {
        private const string AToZ = "atoz";
        private const string Untest = "untest";

        private readonly Memory<double> _input;
        private readonly Memory<double> _output;
        private readonly Memory<double> _expectedOutput;

        public IndividualFunctionTests()
        {
            _input = new Memory<double>(new double[1024]);
            _output = new Memory<double>(new double[1024]);
            _expectedOutput = new Memory<double>(new double[1024]);
        }

        [Fact]
        public void Acos()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Acos.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Acos(ref input, ref output, inputSize, out int outputSize);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Ad()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Ad.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Ad(ref input, ref output, inputSize, out int outputSize);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Add()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Add.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Add(ref input, ref output, inputSize, out int outputSize);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void AdOsc()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.AdOsc.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.AdOsc(ref input, ref output, inputSize, out int outputSize, (int)parameters[0], (int)parameters[1]);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Adx()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Adx.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Adx(ref input, ref output, inputSize, out int outputSize, (int) parameters[0]);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Adxr()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Adxr.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Adxr(ref input, ref output, inputSize, out int outputSize, (int) parameters[0]);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Apo()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Apo.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Apo(ref input, ref output, inputSize, out int outputSize, (MAType) parameters[0], (int) parameters[1], (int) parameters[2]);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Aroon()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Aroon.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);
            
            Lib.Aroon(ref input, ref output, inputSize, out int outputSize, (int) parameters[0]);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void AroonOsc()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.AroonOsc.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.AroonOsc(ref input, ref output, inputSize, out int outputSize, (int) parameters[0]);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Asin()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Asin.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Asin(ref input, ref output, inputSize, out int outputSize);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Atan()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Atan.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Atan(ref input, ref output, inputSize, out int outputSize);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Atr()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Atr.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Atr(ref input, ref output, inputSize, out int outputSize, (int) parameters[0]);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void AvgPrice()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.AvgPrice.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.AvgPrice(ref input, ref output, inputSize, out int outputSize);
            output.ShouldMatch(ref expectedOutput);
        }

        //[Fact]
        //public void Bbands()
        //{
        //    var input = _input.Span;
        //    var output = _output.Span;
        //    var expectedOutput = _expectedOutput.Span;

        //    FunctionNames.Bbands.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

        //    Lib.Bbands(ref input, ref output, inputSize, out int outputSize,
        //        (MAType) parameters[0], (int) parameters[1], double.Parse(parameters[2].ToString()), double.Parse(parameters[3].ToString()));
        //    output.ShouldMatch(ref expectedOutput);
        //}

        [Fact]
        public void Ceil()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Ceil.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Ceil(ref input, ref output, inputSize, out int outputSize);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Cos()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Cos.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Cos(ref input, ref output, inputSize, out int outputSize);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Cosh()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Cosh.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Cosh(ref input, ref output, inputSize, out int outputSize);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Dema()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Dema.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Dema(ref input, ref output, inputSize, out int outputSize, (int) parameters[0]);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Div()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Div.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Div(ref input, ref output, inputSize, out int outputSize);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Ema()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Ema.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Ema(ref input, ref output, inputSize, out int outputSize, (int) parameters[0]);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Exp()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Exp.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Exp(ref input, ref output, inputSize, out int outputSize);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Floor()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Floor.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Floor(ref input, ref output, inputSize, out int outputSize);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Kama()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Kama.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Kama(ref input, ref output, inputSize, out int outputSize, (int) parameters[0]);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Ln()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Ln.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Ln(ref input, ref output, inputSize, out int outputSize);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Log10()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Log10.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Log10(ref input, ref output, inputSize, out int outputSize);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Macd()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Macd.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Macd(ref input, ref output, inputSize, out int outputSize, (int) parameters[0], (int) parameters[1], (int) parameters[2]);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Max()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Max.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Max(ref input, ref output, inputSize, out int outputSize, (int) parameters[0]);
            output.ShouldMatch(ref expectedOutput);
        }

        //[Fact]
        //public void Mama()
        //{
        //    var input = _input.Span;
        //    var output = _output.Span;
        //    var expectedOutput = _expectedOutput.Span;

        //    FunctionNames.Mama.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

        //    Lib.Mama(ref input, ref output, inputSize, out int outputSize, (int) parameters[0]);
        //    output.ShouldMatch(ref expectedOutput);
        //}

        [Fact]
        public void Sma()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Sma.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Sma(ref input, ref output, inputSize, out int outputSize, (int) parameters[0]);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Tema()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Tema.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Tema(ref input, ref output, inputSize, out int outputSize, (int) parameters[0]);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Trima()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Trima.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Trima(ref input, ref output, inputSize, out int outputSize, (int) parameters[0]);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Wma()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Wma.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Wma(ref input, ref output, inputSize, out int outputSize, (int) parameters[0]);
            output.ShouldMatch(ref expectedOutput);
        }

        [Fact]
        public void Sub()
        {
            var input = _input.Span;
            var output = _output.Span;
            var expectedOutput = _expectedOutput.Span;

            FunctionNames.Sub.FillTestData(Untest, ref input, ref expectedOutput, out var inputSize, out var parameters);

            Lib.Sub(ref input, ref output, inputSize, out int outputSize);
            output.ShouldMatch(ref expectedOutput);
        }
    }
}
