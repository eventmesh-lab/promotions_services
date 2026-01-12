using MediatR;
using Microsoft.AspNetCore.Mvc;
using promotions_services.application.Commands.Command;
using promotions_services.application.DTOs;
using promotions_services.application.Queries.Handlers;
using promotions_services.application.Queries.Queries;
using promotions_services.domain.Interfaces;

namespace promotions_services.api.Controllers
{
    [ApiController]
    [Route("api/coupons")]
    public class CouponControllers : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICouponRepository _couponRepository;

        public CouponControllers(IMediator mediator, ICouponRepository couponRepository)
        {
            _mediator = mediator;
            _couponRepository = couponRepository;
        }

        [HttpPost("generateCoupon")]
        public async Task<IActionResult> CreateUser([FromBody] GenerateCouponDto request,
            CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrEmpty(request.email))
            {
                return BadRequest(new ResultadoDTO
                {
                    Exito = false,
                    Mensaje = "El cuerpo de la solicitud es inválido o el email está vacío."
                });
            }
            var resultado = await _mediator.Send(new GenerateCouponCommand(request));

            if (resultado.Exito)
            {
                return Ok(resultado);
            }

            return BadRequest(resultado);
        }

        [HttpGet("getCouponsUser/{correo}")]
        public async Task<IActionResult> ObtenerMediosPagoUsuario([FromRoute] string correo)
        {
            var resultado = await _mediator.Send(new GetValidCouponsByUserQuery(correo));
            return Ok(resultado);
        }

        [HttpPost("getCoupon/{id}")]
        public async Task<IActionResult> ObtenerMedioDePago([FromRoute] Guid id)
        {
            var resultado = await _mediator.Send(new GetValidCouponQuery(id));
            return Ok(resultado);
            
        }

        [HttpPut("updateUser/{id}")]
        public async Task<IActionResult> UpdateUser([FromRoute] Guid id)
        {

            var resultado = _couponRepository.UpdateCouponValid(id);
            bool resultadoFinal = await resultado;

            if (resultadoFinal)

            {
                return Ok(new ResultadoDTO { Mensaje = "El cupon se actualizo exitosamente.", Exito = true });
            }

            return BadRequest(new ResultadoDTO { Mensaje = "El cupon no pudo ser actualizado.", Exito = false });
            
        }
    }
}

