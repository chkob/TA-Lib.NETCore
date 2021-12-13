using System.Linq;
using AutoFixture;
using FluentAssertions;
using static TALib.Core;
using NUnit.Framework;

namespace TALib.NETCore.Library.Tests.Indicators.Func
{
    public class StochRsiTests
    {
        [Test]
        public void StochRsiDouble()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            double[] real = fixture.CreateMany<double>(100).ToArray();
            double[] outputRealK = new double[100];
            double[] outputRealD = new double[100];

            // Act
            var actualResult = Core.StochRsi(
                real,
                startIdx,
                endIdx,
                outputRealK,
                outputRealD,
                out outBegIdx,
                out outNbElement);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Should().Be(RetCode.Success);
        }
        
        [Test]
        public void StochRsiDecimal()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            decimal[] real = fixture.CreateMany<decimal>(100).ToArray();
            decimal[] outputRealK = new decimal[100];
            decimal[] outputRealD = new decimal[100];

            // Act
            var actualResult = Core.StochRsi(
                real,
                startIdx,
                endIdx,
                outputRealK,
                outputRealD,
                out outBegIdx,
                out outNbElement);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Should().Be(RetCode.Success);
        }
    }
}
