using Xunit;
using Moq;
using promotions_services.application.Commands.Handlers;
using promotions_services.application.Commands.Command;
using promotions_services.application.DTOs;
using promotions_services.domain.Interfaces;
using promotions_services.domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace promotions_services.application.Tests.Commands.Handlers
{
    public class GenerateCouponHandlerTests
    {
        private readonly Mock<ICouponRepository> _couponRepositoryMock;
        private readonly GenerateCouponHandler _handler;

        public GenerateCouponHandlerTests()
        {
            _couponRepositoryMock = new Mock<ICouponRepository>();
            _handler = new GenerateCouponHandler(_couponRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldGenerateCoupon_WhenNoPreviousCouponExists()
        {
            var email = "newuser@test.com";
            var command = new GenerateCouponCommand(new GenerateCouponDto { email = email });

            _couponRepositoryMock
                .Setup(r => r.GetUltimoCoupon(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Coupon)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.Exito);
            Assert.NotNull(result.coupon);
            Assert.Contains("Felicidades", result.Mensaje);

            _couponRepositoryMock.Verify(r => r.AddUCouponPostgres(It.IsAny<Coupon>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenPreviousCouponCreatedLessThan15DaysAgo()
        {
            var email = "recentuser@test.com";
            var command = new GenerateCouponCommand(new GenerateCouponDto { email = email });

            var recentCoupon = new Coupon(email, 10, 100);

            _couponRepositoryMock
                .Setup(r => r.GetUltimoCoupon(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(recentCoupon);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.False(result.Exito);
            Assert.Null(result.coupon);
            Assert.Contains("días", result.Mensaje);

            _couponRepositoryMock.Verify(r => r.AddUCouponPostgres(It.IsAny<Coupon>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldGenerateCoupon_WhenPreviousCouponCreatedMoreThan15DaysAgo()
        {
            var email = "olduser@test.com";
            var command = new GenerateCouponCommand(new GenerateCouponDto { email = email });

            var oldCoupon = new Coupon(email, 10, 100);

            typeof(Coupon).GetProperty("CreatedAt")?.SetValue(oldCoupon, DateTime.UtcNow.AddDays(-20));

            _couponRepositoryMock
                .Setup(r => r.GetUltimoCoupon(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(oldCoupon);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.Exito);
            Assert.NotNull(result.coupon);
            Assert.Contains("Felicidades", result.Mensaje);

            _couponRepositoryMock.Verify(r => r.AddUCouponPostgres(It.IsAny<Coupon>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowApplicationException_WhenRepositoryThrowsException()
        {
            var email = "error@test.com";
            var command = new GenerateCouponCommand(new GenerateCouponDto { email = email });

            _couponRepositoryMock
                .Setup(r => r.GetUltimoCoupon(email, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            var exception = await Assert.ThrowsAsync<ApplicationException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Ha ocurrido un error al generar el cupon", exception.Message);
        }
    }
}