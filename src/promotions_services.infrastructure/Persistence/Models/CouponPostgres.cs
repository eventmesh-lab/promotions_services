using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace promotions_services.infrastructure.Persistence.Models
{
    public class CouponPostgres
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string DiscountAmount { get; set; }
        public string CreatedAt { get; set; }
        public string ExpirationDate { get; set; }
        public bool IsValid { get; set; }
        public string AmountMin { get; set; }
    }
}
