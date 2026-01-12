using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace promotions_services.domain.Entities
{
    public class CouponUser
    {
        public Guid CouponId { get; set; }
        public Guid UserId { get; set; }
        public DateTime AssignedDate { get; set; }
    }
}
