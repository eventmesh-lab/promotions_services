using MediatR;
using promotions_services.application.DTOs;
using promotions_services.application.Queries.Queries;
using promotions_services.domain.Entities;
using promotions_services.domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace promotions_services.application.Queries.Handlers
{
    public class GetValidCouponsByUserHandler : IRequestHandler<GetValidCouponsByUserQuery, List<GetValidCouponsDto>>
    {
        private readonly ICouponRepository _couponRepository;

        public GetValidCouponsByUserHandler(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }

        public async Task<List<GetValidCouponsDto>> Handle(GetValidCouponsByUserQuery request, CancellationToken cancellationToken)
        {

            try
            {
                /* // Se obtiene el ID del usuario al que le pertenecen los medio de pago dado.
                 var idUsuario = await _usuarioService.ObtenerUsuarioPorEmailAsync(request.correo);
                 Console.WriteLine($"hola {idUsuario}");
                 if (idUsuario == Guid.Empty)
                 {
                     throw new ArgumentException("El usuario no existe en la base de datos");
                 }
                 //En caso de que el ID del usuario retornado por la consulta sea vacío, se lanza la excepción
                 if (idUsuario == Guid.Empty || idUsuario == null)
                     throw new ApplicationException($"El usuario no existe en la base de datos.");*/
                var coupons =await _couponRepository.GetCouponsByUserAsync(request.Email, cancellationToken);

                //En caso de que cupones esté vacia, se lanza la excepción
                if (coupons == null)
                {
                    throw new ApplicationException($"No existen cupones asociados al usuario.");
                }
                var listaCoupons = new List<GetValidCouponsDto>();
                Console.WriteLine(coupons);
                foreach (var coupon in coupons)
                {
                    bool si = (coupon.ExpirationDate > DateTime.Today);
                    Console.WriteLine(si);
                    if (coupon.IsValid && (coupon.ExpirationDate > DateTime.Today))
                    {
                        listaCoupons.Add(new GetValidCouponsDto
                        {
                            Id = coupon.Id,
                            Email = coupon.Email,
                            DiscountAmount = coupon.DiscountAmount,
                            AmountMin = coupon.AmountMin,
                            IsValid = coupon.IsValid,
                            CreatedAt = coupon.CreatedAt,
                            ExpirationDate = coupon.ExpirationDate

                        });
                    }
                }


                return listaCoupons;

            }
            catch (System.Exception ex)
            {
                throw new ApplicationException("Ha ocurrido un error al obtener los cupones validos del usuario", ex);
            }
        }
    }
}
