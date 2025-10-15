using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace MSP.Infrastructure.SeedConfiguration
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<Guid>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> builder)
        {
            builder.HasData(
                new IdentityUserRole<Guid>
                {
                    UserId = Guid.Parse("c1d2e3f4-a5b6-4789-1234-56789abcdef2"), // admin user Id
                    RoleId = Guid.Parse("a1b2c3d4-e5f6-4789-1234-56789abcdef0")  // admin role Id
                },
                new IdentityUserRole<Guid>
                {
                    UserId = Guid.Parse("c2d4e3f4-a5b6-4789-1234-56789abcdef2"), // member user Id
                    RoleId = Guid.Parse("b1c2d3e4-f5a6-4789-1234-56789abcdef1")  // member role Id
                },
                new IdentityUserRole<Guid>
                {
                    UserId = Guid.Parse("c3d4e3f4-a5b6-4789-1234-56789abcdef2"), // project manager user Id
                    RoleId = Guid.Parse("c1d2e3f4-a5b6-4789-1234-56789abcdef2")  // project manager role Id
                },
                new IdentityUserRole<Guid>
                {
                    UserId = Guid.Parse("c4d4e3f4-a5b6-4789-1234-56789abcdef2"), // business owner user Id
                    RoleId = Guid.Parse("d1e2f3a4-b5c6-4789-1234-56789abcdef3")  // business owner role Id
                }
            );
        }
    }
}
