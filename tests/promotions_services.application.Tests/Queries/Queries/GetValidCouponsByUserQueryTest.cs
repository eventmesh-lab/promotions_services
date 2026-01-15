using Xunit;
using promotions_services.application.Queries.Queries;
using promotions_services.application.DTOs;
using MediatR;
using System.Collections.Generic;

namespace promotions_services.application.Tests.Queries
{
    public class GetValidCouponsByUserQueryTests
    {
        [Fact]
        public void Constructor_ShouldInitializeEmailCorrectly()
        {
            var expectedEmail = "test@example.com";

            var query = new GetValidCouponsByUserQuery(expectedEmail);

            Assert.Equal(expectedEmail, query.Email);
        }

        [Fact]
        public void Email_ShouldBeSettable()
        {
            var query = new GetValidCouponsByUserQuery("initial@example.com");
            var newEmail = "updated@example.com";

            query.Email = newEmail;

            Assert.Equal(newEmail, query.Email);
        }

        [Fact]
        public void ShouldImplementIRequestInterface()
        {
            var query = new GetValidCouponsByUserQuery("test@example.com");

            Assert.IsAssignableFrom<IRequest<List<GetValidCouponsDto>>>(query);
        }
    }
}