using System.Linq;
using AutoFixture;
using FluentAssertions;
using static TALib.Core;
using NUnit.Framework;

namespace TALib.NETCore.Library.Tests.Indicators.Func
{
    public class HtPhasorTests
    {
        [Test]
        public void HtPhasorDouble()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            double[] real = fixture.CreateMany<double>(100).ToArray();
            double[] outputInPhase = new double[100];
            double[] outputQuadrature = new double[100];

            // Act
            var actualResult = Core.HtPhasor(
                real,
                startIdx,
                endIdx,
                outputInPhase,
                outputQuadrature,
                out outBegIdx,
                out outNbElement);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Should().Be(RetCode.Success);
        }
        
        [Test]
        public void HtPhasorDecimal()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            decimal[] real = fixture.CreateMany<decimal>(100).ToArray();
            decimal[] outputInPhase = new decimal[100];
            decimal[] outputQuadrature = new decimal[100];

            // Act
            var actualResult = Core.HtPhasor(
                real,
                startIdx,
                endIdx,
                outputInPhase,
                outputQuadrature,
                out outBegIdx,
                out outNbElement);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Should().Be(RetCode.Success);
        }
    }
}
