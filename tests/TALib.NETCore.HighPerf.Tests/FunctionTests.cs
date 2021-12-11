using System;
using System.Globalization;
using System.Linq;
using Shouldly;
using TALib.NETCore.HighPerf.Tests.Models;
using Xunit;

namespace TALib.NETCore.HighPerf.Tests
{
    public class FunctionTests
    {
        private const double EqualityTolerance = 0.001d;

        [SkippableTheory]
        [JsonFileData("DataSets/untest.json", "_")]
        [JsonFileData("DataSets/atoz.json", "_")]
        [JsonFileData("DataSets/extra.json", "_")]
        public void ShouldReturnCorrectOutputWithOkStatusForDoubleInput(TestDataModel model, string fileName)
        {
            Skip.If(model.Skip, "Test has been skipped in configuration");

            Functions.Contains(model.Name).ShouldBeTrue($"Cannot find definition for '{model.Name}");
            var function = Functions.Get(model.Name);

            model.Options.Length.ShouldBe(function.Options.Length, "Number of options must match the definition");
            var inputOffset = function.Lookback(Array.ConvertAll(model.Options, d => (int) d));
            model.Inputs.Length.ShouldBe(function.Inputs.Length, "Number of inputs must match the definition");
            var outputLength = model.Inputs[0].Length - inputOffset;
            outputLength.ShouldBePositive("Output array should have the correct length");

            var resultOutput = new double[model.Outputs.Length][];
            resultOutput.Length.ShouldBe(function.Outputs.Length, "Number of outputs must match the definition");
            for (var i = 0; i < resultOutput.Length; i++)
            {
                resultOutput[i] = new double[outputLength];
            }

            var returnCode = function.Run(model.Inputs, model.Options, resultOutput);
            returnCode.ShouldBe(RetCode.Success, "Function should complete with success status code RetCode.Success(0)");

            for (var i = 0; i < resultOutput.Length; i++)
            {
                var f = string.Join(", ", resultOutput[0].Select(n => Math.Round(n, 4).ToString(CultureInfo.GetCultureInfo("en-US"))));
                resultOutput[i].Length.ShouldBe(model.Outputs[i].Length,
                    $"Expected and calculated length of the output values should be equal for output {i + 1}");
                resultOutput[i].ShouldBe(model.Outputs[i], EqualityTolerance,
                    $"Calculated values should be within expected for output {i + 1}");
            }
        }
    }
}
