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
                        Mensaje = $"¡Felicidades! Se ha generado un cupón del {coupon.DiscountAmount}% de descuento para un monto minimo de {coupon.AmountMin}. Puedes canjear hasta el {fecha} ",
                        coupon = new GetValidCouponsDto{
                                        Id= coupon.Id,
                                        Email= coupon.Email,
                                        DiscountAmount= coupon.DiscountAmount,
                                        CreatedAt= coupon.CreatedAt,
                                        ExpirationDate= coupon.ExpirationDate,
                                        IsValid= coupon.IsValid,
                                        AmountMin= coupon.AmountMin
                                    }
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
                            Mensaje = $"Ya has solicitado un cupón recientemente. Podrás generar uno nuevo en {diasRestantes} días.",
                            coupon = null
                        };
                    }
                }
               
                await _coupon.AddUCouponPostgres(coupon, cancellationToken);
                
                return new ResultadoDTO
                {
                    Exito = true,
                    Mensaje = $"¡Felicidades! Se ha generado un cupón del {coupon.DiscountAmount}% de descuento para un monto minimo de {coupon.AmountMin}. Puedes canjear hasta el {fecha} ",
                    coupon = new GetValidCouponsDto
                    {
                        Id = coupon.Id,
                        Email = coupon.Email,
                        DiscountAmount = coupon.DiscountAmount,
                        CreatedAt = coupon.CreatedAt,
                        ExpirationDate = coupon.ExpirationDate,
                        IsValid = coupon.IsValid,
                        AmountMin = coupon.AmountMin
                    }
                };
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ha ocurrido un error al generar el cupon", ex);
            }
        }
    }
}
