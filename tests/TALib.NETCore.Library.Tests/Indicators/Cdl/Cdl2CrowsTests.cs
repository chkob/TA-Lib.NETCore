using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using static TALib.Core;

namespace TALib.NETCore.Library.Tests.Indicators.Cdl
{
    public class Cdl2CrowsTests
    {
        [Test]
        public void Cdl2CrowsDouble()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            double[] open = fixture.CreateMany<double>(100).ToArray();
            double[] high = fixture.CreateMany<double>(100).ToArray();
            double[] low = fixture.CreateMany<double>(100).ToArray();
            double[] close = fixture.CreateMany<double>(100).ToArray();
            int[] output = new int[100];
 
            // Act
            var actualResult = Core.Cdl2Crows(
                open,
                high,
                low,
                close,
                startIdx,
                endIdx,
                output,
                out outBegIdx,
                out outNbElement);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Should().Be(RetCode.Success);
        }

        [Test]
        public void Cdl2CrowsDecimal()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            decimal[] open = fixture.CreateMany<decimal>(100).ToArray();
            decimal[] high = fixture.CreateMany<decimal>(100).ToArray();
            decimal[] low = fixture.CreateMany<decimal>(100).ToArray();
            decimal[] close = fixture.CreateMany<decimal>(100).ToArray();
            int[] output = new int[100];
 
            // Act
            var actualResult = Core.Cdl2Crows(
                open,
                high,
                low,
                close,
                startIdx,
                endIdx,
                output,
                out outBegIdx,
                out outNbElement);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Should().Be(RetCode.Success);
        }
    }
}