using System.Linq;
using AutoFixture;
using FluentAssertions;
using static TALib.Core;
using NUnit.Framework;

namespace TALib.NETCore.Library.Tests.Indicators.Func
{
    public class MacdExtTests
    {
        [Test]
        public void MacdExtDouble()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            double[] real = fixture.CreateMany<double>(100).ToArray();
            double[] outputReal = new double[100];
            double[] outputSignal = new double[100];
            double[] outputHistory = new double[100];

            // Act
            var actualResult = Core.MacdExt(
                real,
                startIdx,
                endIdx,
                outputReal,
                outputSignal,
                outputHistory,
                out outBegIdx,
                out outNbElement);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Should().Be(RetCode.Success);
        }
        
        [Test]
        public void MacdExtDecimal()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            decimal[] real = fixture.CreateMany<decimal>(100).ToArray();
            decimal[] outputReal = new decimal[100];
            decimal[] outputSignal = new decimal[100];
            decimal[] outputHistory = new decimal[100];

            // Act
            var actualResult = Core.MacdExt(
                real,
                startIdx,
                endIdx,
                outputReal,
                outputSignal,
                outputHistory,
                out outBegIdx,
                out outNbElement);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Should().Be(RetCode.Success);
        }
    }
}
