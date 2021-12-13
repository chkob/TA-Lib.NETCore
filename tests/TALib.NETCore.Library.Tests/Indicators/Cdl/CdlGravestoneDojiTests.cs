using System.Linq;
using AutoFixture;
using FluentAssertions;
using static TALib.Core;
using NUnit.Framework;

namespace TALib.NETCore.Library.Tests.Indicators.Cdl
{
    public class CdlGravestoneDojiTests
    {
        [Test]
        public void CdlGravestoneDojiDouble()
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
            var actualResult = Core.CdlGravestoneDoji(
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
        public void CdlGravestoneDojiDecimal()
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
            var actualResult = Core.CdlGravestoneDoji(
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
