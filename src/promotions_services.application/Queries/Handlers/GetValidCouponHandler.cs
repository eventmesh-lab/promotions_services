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
    public class GetValidCouponHandler : IRequestHandler<GetValidCouponQuery, GetValidCouponsDto>
    {

        private readonly ICouponRepository _couponRepository;
        public GetValidCouponHandler(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }

        public async Task<GetValidCouponsDto> Handle(GetValidCouponQuery request, CancellationToken cancellationToken)
        {

            try
            {

                //Se obtiene el cupon en la base de datos 
                var coupon = await _couponRepository.GetCouponById(request.Id, cancellationToken);

                //En caso de que el cupon esté vacio, se lanza la excepción
                if (coupon == null)
                    throw new ApplicationException($"No existen este cupon.");

                var cuponDto = new GetValidCouponsDto
                {
                    Id = coupon.Id,
                    Email = coupon.Email,
                    DiscountAmount = coupon.DiscountAmount,
                    AmountMin = coupon.AmountMin,
                    IsValid = coupon.IsValid,
                    CreatedAt = coupon.CreatedAt,
                    ExpirationDate = coupon.ExpirationDate
                };

                return cuponDto;

            }
            catch (System.Exception ex)
            {
                throw new ApplicationException("Ha ocurrido un error al el cupon", ex);
            }
        }
    }
}
