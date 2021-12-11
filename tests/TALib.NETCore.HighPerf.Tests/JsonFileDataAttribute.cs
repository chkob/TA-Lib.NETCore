using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using TALib.NETCore.HighPerf.Tests.Models;
using Xunit.Sdk;

namespace TALib.NETCore.HighPerf.Tests
{
    public sealed class JsonFileDataAttribute : DataAttribute
    {
        private static readonly Type TargetCollectionType = typeof(IEnumerable<>).MakeGenericType(typeof(TestDataModel));
        
        private readonly string _filePath;
        private readonly string? _propertyName;

        /// <summary>
        /// Load data from a JSON file as the data source for a theory
        /// </summary>
        /// <param name="filePath">The absolute or relative path to the JSON file to load</param>
        /// <param name="propertyName">The name of the property on the JSON file that contains the data for the test</param>
        public JsonFileDataAttribute(string filePath, string? propertyName = null)
        {
            _filePath = filePath;
            _propertyName = propertyName;
        }

        /// <inheritDoc />
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null)
            {
                throw new ArgumentNullException(nameof(testMethod));
            }

            var path = Path.IsPathRooted(_filePath)
                ? _filePath
                : Path.GetRelativePath(Directory.GetCurrentDirectory(), _filePath);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find file at path {path}");
            }

            IEnumerable<object> dataModels;

            using (var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
            using (var dataDocument = JsonDocument.Parse(fileStream))
            {
                if (!string.IsNullOrEmpty(_propertyName))
                {
                    if (!dataDocument.RootElement.TryGetProperty(_propertyName.AsSpan(), out var dataProperty) ||
                        dataProperty.ValueKind != JsonValueKind.Array)
                    {
                        throw new JsonException($"Could not find property {_propertyName}");
                    }

                    dataModels = (IEnumerable<object>) dataProperty.ToObject(TargetCollectionType,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    dataModels = (IEnumerable<object>) dataDocument.ToObject(TargetCollectionType,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }

            foreach (object dataModel in dataModels)
            {
                yield return new[] { dataModel, Path.GetFileName(_filePath) };
            }
        }
    }
}
