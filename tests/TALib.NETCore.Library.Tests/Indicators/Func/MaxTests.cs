using System.Linq;
using AutoFixture;
using FluentAssertions;
using static TALib.Core;
using NUnit.Framework;

namespace TALib.NETCore.Library.Tests.Indicators.Func
{
    public class MaxTests
    {
        [Test]
        public void MaxDouble()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            double[] real = fixture.CreateMany<double>(100).ToArray();
            double[] outputReal = new double[100];

            // Act
            var actualResult = Core.Max(
                real,
                startIdx,
                endIdx,
                outputReal,
                out outBegIdx,
                out outNbElement);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Should().Be(RetCode.Success);
        }
        
        [Test]
        public void MaxDecimal()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            decimal[] real = fixture.CreateMany<decimal>(100).ToArray();
            decimal[] outputReal = new decimal[100];

            // Act
            var actualResult = Core.Max(
                real,
                startIdx,
                endIdx,
                outputReal,
                out outBegIdx,
                out outNbElement);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Should().Be(RetCode.Success);
        }
    }
}
