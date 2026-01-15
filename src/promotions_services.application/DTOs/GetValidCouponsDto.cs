using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace promotions_services.application.DTOs
{
    public class GetValidCouponsDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public int DiscountAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsValid { get; set; }
        public decimal AmountMin { get; set; }
    }
}
