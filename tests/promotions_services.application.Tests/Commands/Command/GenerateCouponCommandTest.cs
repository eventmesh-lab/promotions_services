using Xunit;
using promotions_services.application.Commands.Command;
using promotions_services.application.DTOs;
using MediatR;

namespace promotions_services.application.Tests.Commands
{
    public class GenerateCouponCommandTests
    {
        [Fact]
        public void Constructor_ShouldInitializeDtoCorrectly()
        {
            var expectedDto = new GenerateCouponDto { email = "test@example.com" };

            var command = new GenerateCouponCommand(expectedDto);

            Assert.Same(expectedDto, command.Dto);
            Assert.Equal("test@example.com", command.Dto.email);
        }

        [Fact]
        public void Dto_ShouldBeSettable()
        {
            var initialDto = new GenerateCouponDto { email = "initial@example.com" };
            var command = new GenerateCouponCommand(initialDto);
            var newDto = new GenerateCouponDto { email = "new@example.com" };

            command.Dto = newDto;

            Assert.Same(newDto, command.Dto);
            Assert.Equal("new@example.com", command.Dto.email);
        }

        [Fact]
        public void ShouldImplementIRequestInterface()
        {
            var dto = new GenerateCouponDto();
            var command = new GenerateCouponCommand(dto);

            Assert.IsAssignableFrom<IRequest<ResultadoDTO>>(command);
        }
    }
}