using System.Linq;
using AutoFixture;
using FluentAssertions;
using static TALib.Core;
using NUnit.Framework;

namespace TALib.NETCore.Library.Tests.Indicators.Func
{
    public class StochTests
    {
        [Test]
        public void StochDouble()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            double[] high = fixture.CreateMany<double>(100).ToArray();
            double[] low = fixture.CreateMany<double>(100).ToArray();
            double[] close = fixture.CreateMany<double>(100).ToArray();
            double[] outputRealK = new double[100];
            double[] outputRealD = new double[100];

            // Act
            var actualResult = Core.Stoch(
                high,
                low,
                close,
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
        public void StochDecimal()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            decimal[] high = fixture.CreateMany<decimal>(100).ToArray();
            decimal[] low = fixture.CreateMany<decimal>(100).ToArray();
            decimal[] close = fixture.CreateMany<decimal>(100).ToArray();
            decimal[] outputRealK = new decimal[100];
            decimal[] outputRealD = new decimal[100];

            // Act
            var actualResult = Core.Stoch(
                high,
                low,
                close,
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
