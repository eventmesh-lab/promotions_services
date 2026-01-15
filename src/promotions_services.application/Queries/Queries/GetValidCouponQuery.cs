using MediatR;
using promotions_services.application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace promotions_services.application.Queries.Queries
{
    public class GetValidCouponQuery : IRequest<GetValidCouponsDto>
    {
        public Guid Id { get; set; }

        public GetValidCouponQuery(Guid id)
        {
            Id = id;
        }
    }
}
