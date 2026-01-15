using Microsoft.EntityFrameworkCore;
using promotions_services.domain.Entities;
using promotions_services.infrastructure.Persistence.Context;
using promotions_services.infrastructure.Persistence.Models;
using promotions_services.infrastructure.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace promotions_services.infrastructure.Tests.Persistence.Repositories
{
    public class CouponRepositoryTests
    {
        private AppDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CouponRepository(null));
        }

        [Fact]
        public async Task AddUCouponPostgres_ShouldAddCouponToDatabase()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetDbContext(dbName))
            {
                var repository = new CouponRepository(context);
                var coupon = new Coupon("test@test.com", 10, 100);

                await repository.AddUCouponPostgres(coupon, CancellationToken.None);
            }

            using (var context = GetDbContext(dbName))
            {
                var count = await context.Coupons.CountAsync();
                var savedCoupon = await context.Coupons.FirstOrDefaultAsync();

                Assert.Equal(1, count);
                Assert.Equal("test@test.com", savedCoupon.Email);
            }
        }

        [Fact]
        public async Task GetCouponsByUserAsync_ShouldReturnCoupons_WhenUserHasCoupons()
        {
            var dbName = Guid.NewGuid().ToString();
            var email = "user@test.com";

            using (var context = GetDbContext(dbName))
            {
                context.Coupons.Add(new CouponPostgres
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    DiscountAmount = "10",
                    CreatedAt = DateTime.UtcNow.ToString("o"),
                    ExpirationDate = DateTime.UtcNow.AddDays(10).ToString("o"),
                    AmountMin = "100",
                    IsValid = true
                });
                context.Coupons.Add(new CouponPostgres
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    DiscountAmount = "20",
                    CreatedAt = DateTime.UtcNow.ToString("o"),
                    ExpirationDate = DateTime.UtcNow.AddDays(10).ToString("o"),
                    AmountMin = "100",
                    IsValid = true
                });
                context.Coupons.Add(new CouponPostgres
                {
                    Id = Guid.NewGuid(),
                    Email = "other@test.com",
                    DiscountAmount = "30",
                    CreatedAt = DateTime.UtcNow.ToString("o"),
                    ExpirationDate = DateTime.UtcNow.AddDays(10).ToString("o"),
                    AmountMin = "100",
                    IsValid = true
                });
                await context.SaveChangesAsync();
            }

            using (var context = GetDbContext(dbName))
            {
                var repository = new CouponRepository(context);
                var result = await repository.GetCouponsByUserAsync(email, CancellationToken.None);

                Assert.Equal(2, result.Count);
                Assert.All(result, c => Assert.Equal(email, c.Email));
            }
        }

        [Fact]
        public async Task GetCouponsByUserAsync_ShouldReturnEmptyList_WhenUserHasNoCoupons()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetDbContext(dbName))
            {
                var repository = new CouponRepository(context);
                var result = await repository.GetCouponsByUserAsync("empty@test.com", CancellationToken.None);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetCouponById_ShouldReturnCoupon_WhenIdExists()
        {
            var dbName = Guid.NewGuid().ToString();
            var id = Guid.NewGuid();

            using (var context = GetDbContext(dbName))
            {
                context.Coupons.Add(new CouponPostgres
                {
                    Id = id,
                    Email = "test@test.com",
                    DiscountAmount = "10",
                    CreatedAt = DateTime.UtcNow.ToString("o"),
                    ExpirationDate = DateTime.UtcNow.AddDays(10).ToString("o"),
                    AmountMin = "50",
                    IsValid = true
                });
                await context.SaveChangesAsync();
            }

            using (var context = GetDbContext(dbName))
            {
                var repository = new CouponRepository(context);
                var result = await repository.GetCouponById(id, CancellationToken.None);

                Assert.NotNull(result);
                Assert.Equal(id, result.Id);
            }
        }

        [Fact]
        public async Task GetCouponById_ShouldReturnNull_WhenIdDoesNotExist()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetDbContext(dbName))
            {
                var repository = new CouponRepository(context);
                var result = await repository.GetCouponById(Guid.NewGuid(), CancellationToken.None);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task UpdateCouponValid_ShouldReturnTrueAndSetIsValidToFalse_WhenCouponExists()
        {
            var dbName = Guid.NewGuid().ToString();
            var id = Guid.NewGuid();

            using (var context = GetDbContext(dbName))
            {
                context.Coupons.Add(new CouponPostgres
                {
                    Id = id,
                    IsValid = true,
                    Email = "test@test.com",
                    DiscountAmount = "10",
                    CreatedAt = DateTime.UtcNow.ToString("o"),
                    ExpirationDate = DateTime.UtcNow.AddDays(10).ToString("o"),
                    AmountMin = "50"
                });
                await context.SaveChangesAsync();
            }

            using (var context = GetDbContext(dbName))
            {
                var repository = new CouponRepository(context);
                var result = await repository.UpdateCouponValid(id);

                Assert.True(result);
            }

            using (var context = GetDbContext(dbName))
            {
                var updatedCoupon = await context.Coupons.FindAsync(id);
                Assert.False(updatedCoupon.IsValid);
            }
        }

        [Fact]
        public async Task UpdateCouponValid_ShouldReturnFalse_WhenCouponDoesNotExist()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetDbContext(dbName))
            {
                var repository = new CouponRepository(context);
                var result = await repository.UpdateCouponValid(Guid.NewGuid());

                Assert.False(result);
            }
        }

        [Fact]
        public async Task GetUltimoCoupon_ShouldReturnLatestCoupon_WhenCouponsExist()
        {
            var dbName = Guid.NewGuid().ToString();
            var email = "user@test.com";

            var oldDateStr = DateTime.UtcNow.AddDays(-10).ToString("o");
            var newDateStr = DateTime.UtcNow.ToString("o");

            using (var context = GetDbContext(dbName))
            {
                context.Coupons.Add(new CouponPostgres
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    CreatedAt = oldDateStr,
                    DiscountAmount = "10",
                    ExpirationDate = DateTime.UtcNow.AddDays(5).ToString("o"),
                    AmountMin = "50",
                    IsValid = true
                });
                context.Coupons.Add(new CouponPostgres
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    CreatedAt = newDateStr,
                    DiscountAmount = "20",
                    ExpirationDate = DateTime.UtcNow.AddDays(15).ToString("o"),
                    AmountMin = "50",
                    IsValid = true
                });
                await context.SaveChangesAsync();
            }

            using (var context = GetDbContext(dbName))
            {
                var repository = new CouponRepository(context);
                var result = await repository.GetUltimoCoupon(email, CancellationToken.None);

                Assert.NotNull(result);
                Assert.Equal(20, result.DiscountAmount);
            }
        }

        [Fact]
        public async Task GetUltimoCoupon_ShouldReturnNull_WhenNoCouponsExist()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetDbContext(dbName))
            {
                var repository = new CouponRepository(context);
                var result = await repository.GetUltimoCoupon("user@test.com", CancellationToken.None);

                Assert.Null(result);
            }
        }
    }
}