using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace TALib.NETCore.HighPerf.Tests
{
    public static class TestExtensions
    {
        private const double EqualityTolerance = 0.001d;

        public static void FillTestData(
            this FunctionNames functionName,
            string testDataSetName,
            ref Span<double> inputs,
            ref Span<double> expectedOutputs,
            out int inputSize,
            out List<object> parameters)
        {
            parameters = new List<object>();
            var json = JsonDocument.Parse(File.ReadAllText($"./DataSets/{testDataSetName}.json")).RootElement;
            var functionElement = json.GetProperty("_").EnumerateArray().FirstOrDefault(p => p.GetProperty("name").GetString() == functionName.ToString());
            if (functionElement.ValueKind == JsonValueKind.Undefined)
            {
                throw new ApplicationException($"No test data found for function {functionName}.");
            }
            var inputValues = functionElement.GetProperty("inputs").EnumerateArray().SelectMany(p => p.EnumerateArray()).Select(p => p.GetDouble()).ToArray();
            inputSize = inputValues.Length / functionElement.GetProperty("inputs").EnumerateArray().Count();

            var expectedOutputValues = functionElement.GetProperty("outputs").EnumerateArray().SelectMany(p => p.EnumerateArray()).Select(p => p.GetDouble()).ToArray();
            inputValues.CopyTo(inputs);
            expectedOutputValues.CopyTo(expectedOutputs);
            expectedOutputs = expectedOutputs.Slice(0, expectedOutputValues.Length);

            if (functionElement.TryGetProperty("options", out var optionsElement))
            {
                foreach (var optionElement in optionsElement.EnumerateArray())
                {
                   if (optionElement.TryGetInt32(out var intOptionValue))
                   {
                       parameters.Add(intOptionValue);
                   }
                   else
                   {
                       parameters.Add(optionElement.GetDouble());
                   }
                }
            }

            inputs = inputs.Slice(0, inputValues.Length);
        }

        public static void ShouldMatch(this Span<double> data, ref Span<double> matches)
        {
            if (data.Length != matches.Length)
            {
                Assert.True(false, "The two sequences do not have the same number of elements.");
            }

            var count = 0;
            foreach (var match in matches)
            {
                if (Math.Abs(data[count] - match) > EqualityTolerance)
                {
                    Assert.True(false, $"The two sequences do not match on element {count}. Original: {data[count]}. Expected: {match}");
                }
                count++;
            }

            Assert.True(true);
        }
    }
}
