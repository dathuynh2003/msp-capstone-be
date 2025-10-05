using MSP.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MSP.Infrastructure.SeedConfiguration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasData(
                new User
                {
                    Id = Guid.Parse("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                    UserName = "admin",
                    FirstName = "System",
                    LastName = "Admin",
                    NormalizedUserName = "ADMIN",
                    Email = "admin@gmail.com",
                    PasswordHash = new PasswordHasher<User>().HashPassword(null, "1")
                },
                new User
                {
                    Id = Guid.Parse("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                    UserName = "Member",
                    FirstName = "System",
                    LastName = "Member",
                    NormalizedUserName = "Member",
                    Email = "member@gmail.com",
                    PasswordHash = new PasswordHasher<User>().HashPassword(null, "1")
                },
                new User
                {
                    Id = Guid.Parse("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                    UserName = "ProjectManager",
                    FirstName = "System",
                    LastName = "ProjectManager",
                    NormalizedUserName = "PROJECTMANAGER",
                    Email = "manager@gmail.com",
                    PasswordHash = new PasswordHasher<User>().HashPassword(null, "1")
                },
                new User
                {
                    Id = Guid.Parse("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                    UserName = "Company",
                    FirstName = "System",
                    LastName = "Company",
                    NormalizedUserName = "COMPANY",
                    Email = "company@gmail.com",
                    PasswordHash = new PasswordHasher<User>().HashPassword(null, "1")
                }
            );
        }
    }
}
