using System.Linq;
using AutoFixture;
using FluentAssertions;
using static TALib.Core;
using NUnit.Framework;

namespace TALib.NETCore.Library.Tests.Indicators.Func
{
    public class MinMaxIndexTests
    {
        [Test]
        public void MinMaxIndexDouble()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            double[] real = fixture.CreateMany<double>(100).ToArray();
            int[] outputMin = new int[100];
            int[] outputMax = new int[100];

            // Act
            var actualResult = Core.MinMaxIndex(
                real,
                startIdx,
                endIdx,
                outputMin,
                outputMax,
                out outBegIdx,
                out outNbElement);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Should().Be(RetCode.Success);
        }
        
        [Test]
        public void MinMaxIndexDecimal()
        {
            // Arrange
            Fixture fixture = new();
            const int startIdx = 0;
            const int endIdx = 99;
            int outBegIdx, outNbElement;
            decimal[] real = fixture.CreateMany<decimal>(100).ToArray();
            int[] outputMin = new int[100];
            int[] outputMax = new int[100];

            // Act
            var actualResult = Core.MinMaxIndex(
                real,
                startIdx,
                endIdx,
                outputMin,
                outputMax,
                out outBegIdx,
                out outNbElement);

            // Assert
            actualResult.Should().NotBeNull();
            actualResult.Should().Be(RetCode.Success);
        }
    }
}
