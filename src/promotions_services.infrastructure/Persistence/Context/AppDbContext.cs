using Microsoft.EntityFrameworkCore;
using promotions_services.infrastructure.Persistence.Configurations;
using promotions_services.infrastructure.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace promotions_services.infrastructure.Persistence.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        /// Colección de entidades CouponPostgres en el contexto de la base de datos.

        public DbSet<CouponPostgres> Coupons { get; set; }  
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CouponConfiguration());
        }

    }
}
