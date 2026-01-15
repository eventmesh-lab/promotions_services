using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using promotions_services.infrastructure.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace promotions_services.infrastructure.Persistence.Configurations
{
    public class CouponConfiguration : IEntityTypeConfiguration<CouponPostgres>
    {
        public void Configure(EntityTypeBuilder<CouponPostgres> builder)
        {
            builder.HasKey(u => u.Id); // Clave primaria

            builder.Property(u => u.Email)
                .IsRequired();

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.Property(u => u.ExpirationDate)
                .IsRequired();

            builder.Property(u => u.AmountMin)
                .IsRequired();

            builder.Property(u => u.DiscountAmount)
                .IsRequired();
            builder.Property(u => u.IsValid)
                .IsRequired();


        }
    }
}
