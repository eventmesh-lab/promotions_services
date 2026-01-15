using Xunit;
using promotions_services.application.Commons;
using promotions_services.domain.Enums;
using System;

namespace promotions_services.application.Tests.Commons
{
    public class GenerateAmountsTests
    {
        [Fact]
        public void GetAmountDiscountRandom_ShouldReturnValidEnumValue()
        {
            var result = GenerateAmounts.GetAmountDiscountRandom();

            Assert.True(Enum.IsDefined(typeof(EnumAmountDiscount), result));
        }

        [Fact]
        public void GetAmountDiscountRandom_ShouldReturnValuesWithinEnumRange_OverMultipleCalls()
        {
            for (int i = 0; i < 50; i++)
            {
                var result = GenerateAmounts.GetAmountDiscountRandom();
                Assert.True(Enum.IsDefined(typeof(EnumAmountDiscount), result));
            }
        }

        [Fact]
        public void GetAmountMixRandom_ShouldReturnValidEnumValue()
        {
            var result = GenerateAmounts.GetAmountMixRandom();

            Assert.True(Enum.IsDefined(typeof(EnumAmountMin), result));
        }

        [Fact]
        public void GetAmountMixRandom_ShouldReturnValuesWithinEnumRange_OverMultipleCalls()
        {
            for (int i = 0; i < 50; i++)
            {
                var result = GenerateAmounts.GetAmountMixRandom();
                Assert.True(Enum.IsDefined(typeof(EnumAmountMin), result));
            }
        }
    }
}