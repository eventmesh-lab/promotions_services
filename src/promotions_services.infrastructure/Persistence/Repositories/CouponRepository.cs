using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using promotions_services.domain.Entities;
using promotions_services.domain.Interfaces;
using promotions_services.infrastructure.Mappers;
using promotions_services.infrastructure.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using promotions_services.infrastructure.Persistence.Models;

namespace promotions_services.infrastructure.Persistence.Repositories
{
    public class CouponRepository: ICouponRepository
    {
        public readonly AppDbContext _context;

        /// Inicializa una nueva instancia del repositorio con el contexto de la base de datos proporcionado.
        public CouponRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddUCouponPostgres(Coupon coupon, CancellationToken cancellationToken)
        {
            var model = CouponMapper.ToPostgres(coupon);
            _context.Coupons.Add(model);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Coupon>> GetCouponsByUserAsync(string email, CancellationToken cancellationToken)
        {
            var couponsModel = await _context.Coupons
                .Where(h => h.Email == email)
                .ToListAsync(cancellationToken);
            var couponsPagos = couponsModel
                .Select(CouponMapper.ToDomain)
                .ToList();
            return couponsPagos;
        }
        public async Task<Coupon?> GetCouponById(Guid id, CancellationToken cancellationToken)
        {
            var couponModel = await _context.Coupons
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            if (couponModel == null)
            {
                return null;
            }
            return CouponMapper.ToDomain(couponModel);
        }
        public async Task<bool> UpdateCouponValid(Guid id)
        {
            Console.WriteLine("hola");
            var coupon = await _context.Set<CouponPostgres>()
                .FirstOrDefaultAsync(u => u.Id == id);
            Console.WriteLine("hola");
            if (coupon == null)
                return false;
            Console.WriteLine("hola");
            coupon.IsValid = false;
            Console.WriteLine("hola");
            _context.Set<CouponPostgres>().Update(coupon);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Coupon?> GetUltimoCoupon(string Email, CancellationToken cancellationToken)
        {

            var ultimoCupon = await _context.Coupons
                .Where(c => c.Email == Email)
                .OrderByDescending(c => c.CreatedAt) // Ordenar del más reciente al más antiguo
                .FirstOrDefaultAsync(cancellationToken);
            if (ultimoCupon == null)
            {
                return null;
            }

            return CouponMapper.ToDomain(ultimoCupon);
        }

    }

    

}
