using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using promotions_services.api.Controllers;
using promotions_services.application.Commands.Command;
using promotions_services.application.Queries.Queries;
using promotions_services.application.DTOs;
using promotions_services.domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace promotions_services.api.Tests.Controllers
{
    public class CouponControllersTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ICouponRepository> _couponRepositoryMock;
        private readonly CouponControllers _controller;

        public CouponControllersTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _couponRepositoryMock = new Mock<ICouponRepository>();
            _controller = new CouponControllers(_mediatorMock.Object, _couponRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenRequestIsNull()
        {
            var result = await _controller.CreateUser(null, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var resultDto = Assert.IsType<ResultadoDTO>(badRequestResult.Value);

            Assert.False(resultDto.Exito);
            Assert.Contains("El cuerpo de la solicitud es inválido", resultDto.Mensaje);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenEmailIsEmpty()
        {
            var request = new GenerateCouponDto { email = "" };

            var result = await _controller.CreateUser(request, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var resultDto = Assert.IsType<ResultadoDTO>(badRequestResult.Value);

            Assert.False(resultDto.Exito);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnOk_WhenMediatorReturnsSuccess()
        {
            var request = new GenerateCouponDto { email = "test@test.com" };

            var expectedCoupon = new GetValidCouponsDto
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                IsValid = true
            };

            var expectedResponse = new ResultadoDTO
            {
                Exito = true,
                Mensaje = "Cupón generado",
                coupon = expectedCoupon
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GenerateCouponCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _controller.CreateUser(request, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultDto = Assert.IsType<ResultadoDTO>(okResult.Value);

            Assert.True(resultDto.Exito);
            Assert.NotNull(resultDto.coupon);
            Assert.Equal("test@test.com", resultDto.coupon.Email);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenMediatorReturnsFailure()
        {
            var request = new GenerateCouponDto { email = "test@test.com" };
            var expectedResponse = new ResultadoDTO
            {
                Exito = false,
                Mensaje = "Error al generar"
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GenerateCouponCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _controller.CreateUser(request, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var resultDto = Assert.IsType<ResultadoDTO>(badRequestResult.Value);

            Assert.False(resultDto.Exito);
        }

        [Fact]
        public async Task ObtenerMediosPagoUsuario_ShouldReturnOk_WithListOfCoupons()
        {
            var email = "user@test.com";
            var expectedCoupons = new List<GetValidCouponsDto>
            {
                new GetValidCouponsDto { Id = Guid.NewGuid(), Email = email, DiscountAmount = 10 },
                new GetValidCouponsDto { Id = Guid.NewGuid(), Email = email, DiscountAmount = 20 }
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetValidCouponsByUserQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCoupons);

            var result = await _controller.ObtenerMediosPagoUsuario(email);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedList = Assert.IsType<List<GetValidCouponsDto>>(okResult.Value);

            Assert.Equal(2, returnedList.Count);
            Assert.Equal(email, returnedList[0].Email);

            _mediatorMock.Verify(m => m.Send(
                It.Is<GetValidCouponsByUserQuery>(q => q.Email == email),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ObtenerMedioDePago_ShouldReturnOk_WithSingleCoupon()
        {
            var id = Guid.NewGuid();
            var expectedCoupon = new GetValidCouponsDto
            {
                Id = id,
                Email = "single@test.com",
                DiscountAmount = 15,
                IsValid = true
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetValidCouponQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCoupon);

            var result = await _controller.ObtenerMedioDePago(id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCoupon = Assert.IsType<GetValidCouponsDto>(okResult.Value);

            Assert.Equal(id, returnedCoupon.Id);
            Assert.Equal(15, returnedCoupon.DiscountAmount);

            _mediatorMock.Verify(m => m.Send(
                It.Is<GetValidCouponQuery>(q => q.Id == id),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenRepositoryReturnsTrue()
        {
            var id = Guid.NewGuid();

            _couponRepositoryMock
                .Setup(r => r.UpdateCouponValid(id))
                .ReturnsAsync(true);

            var result = await _controller.UpdateUser(id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultDto = Assert.IsType<ResultadoDTO>(okResult.Value);

            Assert.True(resultDto.Exito);
            Assert.Equal("El cupon se actualizo exitosamente.", resultDto.Mensaje);

            _couponRepositoryMock.Verify(r => r.UpdateCouponValid(id), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenRepositoryReturnsFalse()
        {
            var id = Guid.NewGuid();

            _couponRepositoryMock
                .Setup(r => r.UpdateCouponValid(id))
                .ReturnsAsync(false);

            var result = await _controller.UpdateUser(id);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var resultDto = Assert.IsType<ResultadoDTO>(badRequestResult.Value);

            Assert.False(resultDto.Exito);
            Assert.Equal("El cupon no pudo ser actualizado.", resultDto.Mensaje);
        }
    }
}