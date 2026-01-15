using Xunit;
using Moq;
using promotions_services.application.Queries.Handlers;
using promotions_services.application.Queries.Queries;
using promotions_services.application.DTOs;
using promotions_services.domain.Interfaces;
using promotions_services.domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace promotions_services.application.Tests.Queries.Handlers
{
    public class GetValidCouponsByUserHandlerTests
    {
        private readonly Mock<ICouponRepository> _couponRepositoryMock;
        private readonly GetValidCouponsByUserHandler _handler;

        public GetValidCouponsByUserHandlerTests()
        {
            _couponRepositoryMock = new Mock<ICouponRepository>();
            _handler = new GetValidCouponsByUserHandler(_couponRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnOnlyValidAndFutureCoupons()
        {
            var email = "user@test.com";
            var query = new GetValidCouponsByUserQuery(email);

            var validCoupon = new Coupon(email, 10, 100);
            typeof(Coupon).GetProperty("IsValid")?.SetValue(validCoupon, true);
            typeof(Coupon).GetProperty("ExpirationDate")?.SetValue(validCoupon, DateTime.Today.AddDays(5));

            var expiredCoupon = new Coupon(email, 20, 100);
            typeof(Coupon).GetProperty("IsValid")?.SetValue(expiredCoupon, true);
            typeof(Coupon).GetProperty("ExpirationDate")?.SetValue(expiredCoupon, DateTime.Today.AddDays(-5));

            var invalidCoupon = new Coupon(email, 30, 100);
            typeof(Coupon).GetProperty("IsValid")?.SetValue(invalidCoupon, false);
            typeof(Coupon).GetProperty("ExpirationDate")?.SetValue(invalidCoupon, DateTime.Today.AddDays(5));

            var couponsList = new List<Coupon> { validCoupon, expiredCoupon, invalidCoupon };

            _couponRepositoryMock
                .Setup(r => r.GetCouponsByUserAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(couponsList);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(validCoupon.DiscountAmount, result[0].DiscountAmount);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenRepositoryReturnsEmptyList()
        {
            var email = "user@test.com";
            var query = new GetValidCouponsByUserQuery(email);

            _couponRepositoryMock
                .Setup(r => r.GetCouponsByUserAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Coupon>());

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_ShouldThrowApplicationException_WhenRepositoryReturnsNull()
        {
            var email = "user@test.com";
            var query = new GetValidCouponsByUserQuery(email);

            _couponRepositoryMock
                .Setup(r => r.GetCouponsByUserAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<Coupon>)null);

            var exception = await Assert.ThrowsAsync<ApplicationException>(() =>
                _handler.Handle(query, CancellationToken.None));

            Assert.Equal("Ha ocurrido un error al obtener los cupones validos del usuario", exception.Message);

            Assert.NotNull(exception.InnerException);
            Assert.Equal("No existen cupones asociados al usuario.", exception.InnerException.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrowApplicationException_WhenRepositoryThrowsException()
        {
            var email = "user@test.com";
            var query = new GetValidCouponsByUserQuery(email);

            _couponRepositoryMock
                .Setup(r => r.GetCouponsByUserAsync(email, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database failure"));

            var exception = await Assert.ThrowsAsync<ApplicationException>(() =>
                _handler.Handle(query, CancellationToken.None));

            Assert.Equal("Ha ocurrido un error al obtener los cupones validos del usuario", exception.Message);
            Assert.NotNull(exception.InnerException);
            Assert.Equal("Database failure", exception.InnerException.Message);
        }
    }
}