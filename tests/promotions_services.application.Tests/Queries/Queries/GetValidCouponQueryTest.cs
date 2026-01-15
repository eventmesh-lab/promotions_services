using Xunit;
using promotions_services.application.Queries.Queries;
using promotions_services.application.DTOs;
using MediatR;
using System;

namespace promotions_services.application.Tests.Queries
{
    public class GetValidCouponQueryTests
    {
        [Fact]
        public void Constructor_ShouldInitializeIdCorrectly()
        {
            var expectedId = Guid.NewGuid();

            var query = new GetValidCouponQuery(expectedId);

            Assert.Equal(expectedId, query.Id);
        }

        [Fact]
        public void Id_ShouldBeSettable()
        {
            var initialId = Guid.NewGuid();
            var query = new GetValidCouponQuery(initialId);
            var newId = Guid.NewGuid();

            query.Id = newId;

            Assert.Equal(newId, query.Id);
        }

        [Fact]
        public void ShouldImplementIRequestInterface()
        {
            var query = new GetValidCouponQuery(Guid.NewGuid());

            Assert.IsAssignableFrom<IRequest<GetValidCouponsDto>>(query);
        }
    }
}