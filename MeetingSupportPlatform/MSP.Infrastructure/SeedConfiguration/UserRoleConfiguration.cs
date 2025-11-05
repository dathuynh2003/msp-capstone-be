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
                    UserId = Guid.Parse("d2a5b3c4-d7e8-4789-1234-56789abcdef3"), // member user Id
                    RoleId = Guid.Parse("b1c2d3e4-f5a6-4789-1234-56789abcdef1")  // member role Id
                },

                new IdentityUserRole<Guid>
                {
                    UserId = Guid.Parse("e3b6c7d8-a9f0-4789-1234-56789abcdef4"), // member user Id
                    RoleId = Guid.Parse("b1c2d3e4-f5a6-4789-1234-56789abcdef1")  // member role Id
                },

                new IdentityUserRole<Guid>
                {
                    UserId = Guid.Parse("f4c7d8e9-b1a2-4789-1234-56789abcdef5"), // member user Id
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
                },
                new IdentityUserRole<Guid>
                {
                    UserId = Guid.Parse("a5b6c7d8-e9f0-4789-1234-56789abcdef6"), // business owner user Id
                    RoleId = Guid.Parse("d1e2f3a4-b5c6-4789-1234-56789abcdef3")  // business owner role Id
                },
                new IdentityUserRole<Guid>
                {
                    UserId = Guid.Parse("b6c7d8e9-f0a1-4789-1234-56789abcdef7"), // business owner user Id
                    RoleId = Guid.Parse("d1e2f3a4-b5c6-4789-1234-56789abcdef3")  // business owner role Id
                }
            );
        }
    }
}
