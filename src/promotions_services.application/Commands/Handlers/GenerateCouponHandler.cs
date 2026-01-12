using MediatR;
using promotions_services.application.Commands.Command;
using promotions_services.application.Commons;
using promotions_services.application.DTOs;
using promotions_services.domain.Entities;
using promotions_services.domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace promotions_services.application.Commands.Handlers
{
    public class GenerateCouponHandler : IRequestHandler<GenerateCouponCommand, ResultadoDTO>
    {
        public readonly ICouponRepository _coupon;
        /// Inicializa una nueva instancia del handler.
        public GenerateCouponHandler(ICouponRepository coupon)
        {
            _coupon = coupon;
        }

        public async Task<ResultadoDTO> Handle(GenerateCouponCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($" {request.Dto.email} ");
                var ultimoCupon = await _coupon.GetUltimoCoupon(request.Dto.email, cancellationToken);
                var amountDiscount = (GenerateAmounts.GetAmountDiscountRandom());
                var amountMin = GenerateAmounts.GetAmountMixRandom();
                var coupon = new Coupon(request.Dto.email, (int)amountDiscount, (decimal)amountMin);
                var fecha = coupon.ExpirationDate.Date;
                if (ultimoCupon == null )
                {
                    await _coupon.AddUCouponPostgres(coupon, cancellationToken);
                    return new ResultadoDTO
                    {
                        Exito = true,
                        Mensaje = $"¡Felicidades! Se ha generado un cupón del {coupon.DiscountAmount}% de descuento para un monto minimo de {coupon.AmountMin}. Puedes canjear hasta el {fecha} "
                    };
                }

                if ((DateTime.UtcNow - ultimoCupon.CreatedAt).TotalDays < 15)
                {
                    var diasDesdeUltimo = (DateTime.UtcNow - ultimoCupon.CreatedAt).TotalDays;

                    if (diasDesdeUltimo < 15)
                    {
                        var diasRestantes = 15 - (int)diasDesdeUltimo;
                        return new ResultadoDTO
                        {
                            Exito = false,
                            Mensaje = $"Ya has solicitado un cupón recientemente. Podrás generar uno nuevo en {diasRestantes} días."
                        };
                    }
                }
               
                await _coupon.AddUCouponPostgres(coupon, cancellationToken);
                
                return new ResultadoDTO
                {
                    Exito = true,
                    Mensaje = $"¡Felicidades! Se ha generado un cupón del {coupon.DiscountAmount}% de descuento para un monto minimo de {coupon.AmountMin}. Puedes canjear hasta el {fecha} "
                };
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ha ocurrido un error al generar el cupon", ex);
            }
        }
    }
}
