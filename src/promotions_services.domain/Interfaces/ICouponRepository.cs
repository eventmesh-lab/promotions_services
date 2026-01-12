using promotions_services.domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace promotions_services.domain.Interfaces
{
    public interface ICouponRepository
    {
        Task AddUCouponPostgres(Coupon coupon, CancellationToken cancellationToken);
        Task<List<Coupon>> GetCouponsByUserAsync(string email, CancellationToken cancellationToken);
        Task<Coupon?> GetCouponById(Guid id, CancellationToken cancellationToken);
        Task<bool> UpdateCouponValid(Guid id);
        Task<Coupon?> GetUltimoCoupon(string Email, CancellationToken cancellationToken);

    }
}
