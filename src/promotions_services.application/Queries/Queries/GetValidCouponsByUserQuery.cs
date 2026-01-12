using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using promotions_services.application.DTOs;

namespace promotions_services.application.Queries.Queries
{
    public class GetValidCouponsByUserQuery : IRequest<List<GetValidCouponsDto>>
    {
        public string Email { get; set;}

        public GetValidCouponsByUserQuery(string email)
        {
            Email = email;
        }
    }
}
