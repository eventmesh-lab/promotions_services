using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace promotions_services.domain.Entities
{
    public class Coupon
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public int DiscountAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsValid { get; set; }
        public decimal AmountMin { get; set; }

        public Coupon( string email, int discountAmount, decimal amountMin)
        {
            Id = Guid.NewGuid();
            Email = email;
            DiscountAmount = discountAmount;
            CreatedAt = DateTime.Today; 
            ExpirationDate = CreatedAt.AddYears(1);
            IsValid = true;
            AmountMin = amountMin;
        }

        public Coupon(Guid id, string email, int discountAmount, DateTime createdAt, DateTime expirationDate,
            bool isValid, decimal amountMin)
        {
            Id = id;
            Email = email;
            DiscountAmount = discountAmount;
            CreatedAt = createdAt;
            ExpirationDate = expirationDate;
            IsValid = isValid;
            AmountMin = amountMin;
        }

    }
}
