using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using promotions_services.domain.Entities;
using promotions_services.infrastructure.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace promotions_services.infrastructure.Mappers
{
    public static class CouponMapper
    {
        public static Coupon? ToDomain(CouponPostgres? model)
        {
            if (model == null)
            {
                return null;
            }
            DateTime fechaExpired = DateTime.Parse(model.ExpirationDate);

            DateTime soloFechaExpired = fechaExpired.Date;

            DateTime fechaInicio = DateTime.Parse(model.CreatedAt);

            DateTime soloFechaInicio = fechaInicio.Date;


            return new Coupon(model.Id, model.Email, int.Parse(model.DiscountAmount), soloFechaInicio, soloFechaExpired,
                                model.IsValid, decimal.Parse(model.AmountMin));
        }
        /// Mapea un User a un CouponPostgres.
        public static CouponPostgres ToPostgres(Coupon coupon)
        {
            return new CouponPostgres
            {
                Id=coupon.Id,
                Email = coupon.Email,
                DiscountAmount = coupon.DiscountAmount.ToString(),
                CreatedAt = coupon.CreatedAt.ToString("dd/MM/yyyy"),
                ExpirationDate = coupon.ExpirationDate.ToString("dd/MM/yyyy"),
                IsValid = coupon.IsValid,
                AmountMin = coupon.AmountMin.ToString()
            };
        }
    }
}
