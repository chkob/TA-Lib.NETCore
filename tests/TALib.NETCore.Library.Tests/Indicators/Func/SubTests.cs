using System.Linq;
using AutoFixture;
using FluentAssertions;
using static TALib.Core;
using NUnit.Framework;

namespace TALib.NETCore.Library.Tests.Indicators.Func
{
    public class SubTests
    {
        [Test]
        public void SubDouble()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            double[] real0 = fixture.CreateMany<double>(100).ToArray();
            double[] real1 = fixture.CreateMany<double>(100).ToArray();
            double[] outputReal = new double[100];

            // Act
            var actualResult = Core.Sub(
                real0,
                real1,
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
        public void SubDecimal()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            decimal[] real0 = fixture.CreateMany<decimal>(100).ToArray();
            decimal[] real1 = fixture.CreateMany<decimal>(100).ToArray();
            decimal[] outputReal = new decimal[100];

            // Act
            var actualResult = Core.Sub(
                real0,
                real1,
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