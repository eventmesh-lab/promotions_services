using MediatR;
using promotions_services.application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using promotions_services.domain.Entities;

namespace promotions_services.application.Commands.Command
{
    public class GenerateCouponCommand : IRequest<ResultadoDTO>
    {
        public GenerateCouponDto Dto { get; set; }
        public GenerateCouponCommand(GenerateCouponDto dto)
        {
            Dto = dto;
        }
    }
}
