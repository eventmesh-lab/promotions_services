using Xunit;
using Moq;
using promotions_services.application.Queries.Handlers;
using promotions_services.application.Queries.Queries;
using promotions_services.domain.Interfaces;
using promotions_services.domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace promotions_services.application.Tests.Queries.Handlers
{
    public class GetValidCouponHandlerTests
    {
        private readonly Mock<ICouponRepository> _couponRepositoryMock;
        private readonly GetValidCouponHandler _handler;

        public GetValidCouponHandlerTests()
        {
            _couponRepositoryMock = new Mock<ICouponRepository>();
            _handler = new GetValidCouponHandler(_couponRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnDto_WhenCouponExists()
        {
            var id = Guid.NewGuid();
            var query = new GetValidCouponQuery(id);

            var expectedCoupon = new Coupon("test@test.com", 20, 100);

            typeof(Coupon).GetProperty("Id")?.SetValue(expectedCoupon, id);

            _couponRepositoryMock
                .Setup(r => r.GetCouponById(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCoupon);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(expectedCoupon.Id, result.Id);
            Assert.Equal(expectedCoupon.Email, result.Email);
            Assert.Equal(expectedCoupon.DiscountAmount, result.DiscountAmount);
            Assert.Equal(expectedCoupon.AmountMin, result.AmountMin);
        }

        [Fact]
        public async Task Handle_ShouldThrowApplicationException_WhenCouponDoesNotExist()
        {
            var id = Guid.NewGuid();
            var query = new GetValidCouponQuery(id);

            _couponRepositoryMock
                .Setup(r => r.GetCouponById(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Coupon)null);

            var exception = await Assert.ThrowsAsync<ApplicationException>(() =>
                _handler.Handle(query, CancellationToken.None));

            Assert.Equal("Ha ocurrido un error al el cupon", exception.Message);

            Assert.NotNull(exception.InnerException);
            Assert.Equal("No existen este cupon.", exception.InnerException.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrowApplicationException_WhenRepositoryThrowsException()
        {
            var id = Guid.NewGuid();
            var query = new GetValidCouponQuery(id);

            _couponRepositoryMock
                .Setup(r => r.GetCouponById(id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var exception = await Assert.ThrowsAsync<ApplicationException>(() =>
                _handler.Handle(query, CancellationToken.None));

            Assert.Contains("Ha ocurrido un error al el cupon", exception.Message);
            Assert.NotNull(exception.InnerException);
            Assert.Equal("Database error", exception.InnerException.Message);
        }
    }
}