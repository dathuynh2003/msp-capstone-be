using MSP.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MSP.Infrastructure.SeedConfiguration
{
    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole<Guid>>
    {
        public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder)
        {
            builder.HasData(
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("a1b2c3d4-e5f6-4789-1234-56789abcdef0"),
                    Name = UserRoleEnum.Admin.ToString(),
                    NormalizedName = UserRoleEnum.Admin.ToString().ToUpper()
                },
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("b1c2d3e4-f5a6-4789-1234-56789abcdef1"),
                    Name = UserRoleEnum.Member.ToString(),
                    NormalizedName = UserRoleEnum.Member.ToString().ToUpper()
                },
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                    Name = UserRoleEnum.ProjectManager.ToString(),
                    NormalizedName = UserRoleEnum.ProjectManager.ToString().ToUpper()
                },
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("d1e2f3a4-b5c6-4789-1234-56789abcdef3"),
                    Name = UserRoleEnum.Company.ToString(),
                    NormalizedName = UserRoleEnum.Company.ToString().ToUpper()
                }
            );
        }
    }
}
