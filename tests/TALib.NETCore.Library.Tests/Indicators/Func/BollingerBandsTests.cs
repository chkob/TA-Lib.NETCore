using System.Linq;
using AutoFixture;
using FluentAssertions;
using static TALib.Core;
using NUnit.Framework;

namespace TALib.NETCore.Library.Tests.Indicators.Func
{
    public class BollingerBandsTests
    {
        [Test]
        public void BollingerBandsDouble()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            double[] real = fixture.CreateMany<double>(100).ToArray();
            double[] outputRealUpper = new double[100];
            double[] outputRealMiddle = new double[100];
            double[] outputRealLower = new double[100];

            // Act
            var actualResult = Core.Bbands(
                real,
                startIdx,
                endIdx,
                outputRealUpper,
                outputRealMiddle,
                outputRealLower,
                out outBegIdx,
                out outNbElement);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Should().Be(RetCode.Success);
        }
        
        [Test]
        public void BollingerBandsDecimal()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            decimal[] real = fixture.CreateMany<decimal>(100).ToArray();
            decimal[] outputRealUpper = new decimal[100];
            decimal[] outputRealMiddle = new decimal[100];
            decimal[] outputRealLower = new decimal[100];

            // Act
            var actualResult = Core.Bbands(
                real,
                startIdx,
                endIdx,
                outputRealUpper,
                outputRealMiddle,
                outputRealLower,
                out outBegIdx,
                out outNbElement);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Should().Be(RetCode.Success);
        }
    }
}
