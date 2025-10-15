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
            var hasher = new PasswordHasher<User>();

            var adminUser = new User
            {
                Id = Guid.Parse("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                UserName = "admin",
                FullName = "Admin-Nguyễn Văn An",
                NormalizedUserName = "ADMIN",
                Email = "admin@gmail.com",
                NormalizedEmail = "ADMIN@GMAIL.COM",
                EmailConfirmed = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "1");

            var memberUser = new User
            {
                Id = Guid.Parse("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                UserName = "Member",
                FullName = "Member-Lê Thị Thúy",
                NormalizedUserName = "MEMBER",
                Email = "member@gmail.com",
                NormalizedEmail = "MEMBER@GMAIL.COM",
                EmailConfirmed = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
            memberUser.PasswordHash = hasher.HashPassword(memberUser, "1");

            var pmUser = new User
            {
                Id = Guid.Parse("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                UserName = "ProjectManager",
                FullName = "ProjectManager-Trần Văn Bình",
                NormalizedUserName = "PROJECTMANAGER",
                Email = "manager@gmail.com",
                NormalizedEmail = "MANAGER@GMAIL.COM",
                EmailConfirmed = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
            pmUser.PasswordHash = hasher.HashPassword(pmUser, "1");

            var boUser = new User
            {
                Id = Guid.Parse("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                UserName = "BusinessOwner",
                FullName = "BusinessOwner-Ngô Văn Thanh",
                NormalizedUserName = "BUSINESSOWNER",
                Organization = "FPT Software",
                IsApproved = true,
                IsActive = true,
                Email = "businessowner@gmail.com",
                NormalizedEmail = "BUSINESSOWNER@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
            boUser.PasswordHash = hasher.HashPassword(boUser, "1");

            builder.HasData(adminUser, memberUser, pmUser, boUser);
        }
    }
}
