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

            // --- ADMIN ---
            var adminUser = new User
            {
                Id = Guid.Parse("c1d2e3f4-a5b6-4789-1234-56789abcdef2"),
                UserName = "admin",
                FullName = "Nguyen Van An",
                NormalizedUserName = "ADMIN",
                Email = "admin@gmail.com",
                NormalizedEmail = "ADMIN@GMAIL.COM",
                EmailConfirmed = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "1");

            // --- MEMBER 1 ---
            var memberUser1 = new User
            {
                Id = Guid.Parse("c2d4e3f4-a5b6-4789-1234-56789abcdef2"),
                UserName = "Member",
                FullName = "Le Thi Thuy",
                NormalizedUserName = "MEMBER",
                Email = "member1@gmail.com",
                NormalizedEmail = "MEMBER1@GMAIL.COM",
                EmailConfirmed = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
            memberUser1.PasswordHash = hasher.HashPassword(memberUser1, "1");

            // --- MEMBER 2 ---
            var memberUser2 = new User
            {
                Id = Guid.Parse("d2a5b3c4-d7e8-4789-1234-56789abcdef3"),
                UserName = "Member2",
                FullName = "Pham Minh Hieu",
                NormalizedUserName = "MEMBER2",
                Email = "member2@gmail.com",
                NormalizedEmail = "MEMBER2@GMAIL.COM",
                EmailConfirmed = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
            memberUser2.PasswordHash = hasher.HashPassword(memberUser2, "1");

    
            var memberUser3 = new User
            {
                Id = Guid.Parse("e3b6c7d8-a9f0-4789-1234-56789abcdef4"),
                UserName = "Member3",
                FullName = "Nguyen Bao Chau",
                NormalizedUserName = "MEMBER3",
                Email = "member3@gmail.com",
                NormalizedEmail = "MEMBER3@GMAIL.COM",
                EmailConfirmed = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
            memberUser3.PasswordHash = hasher.HashPassword(memberUser3, "1");

            // --- MEMBER 4 ---
            var memberUser4 = new User
            {
                Id = Guid.Parse("f4c7d8e9-b1a2-4789-1234-56789abcdef5"),
                UserName = "Member4",
                FullName = "Do Thi Lan Anh",
                NormalizedUserName = "MEMBER4",
                Email = "member4@gmail.com",
                NormalizedEmail = "MEMBER4@GMAIL.COM",
                EmailConfirmed = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
            memberUser4.PasswordHash = hasher.HashPassword(memberUser4, "1");

            // --- PROJECT MANAGER ---
            var pmUser = new User
            {
                Id = Guid.Parse("c3d4e3f4-a5b6-4789-1234-56789abcdef2"),
                UserName = "ProjectManager",
                FullName = "Tran Van Binh",
                NormalizedUserName = "PROJECTMANAGER",
                Email = "pm@gmail.com",
                NormalizedEmail = "PM@GMAIL.COM",
                EmailConfirmed = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
            pmUser.PasswordHash = hasher.HashPassword(pmUser, "1");

            // --- BUSINESS OWNER 1 ---
            var boUser1 = new User
            {
                Id = Guid.Parse("c4d4e3f4-a5b6-4789-1234-56789abcdef2"),
                UserName = "BusinessOwner",
                FullName = "Ngo Van Thanh",
                NormalizedUserName = "BUSINESSOWNER",
                Organization = "FPT Software",
                IsApproved = true,
                IsActive = true,
                Email = "bo1@gmail.com",
                NormalizedEmail = "BO1@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
            boUser1.PasswordHash = hasher.HashPassword(boUser1, "1");

            // --- BUSINESS OWNER 2 ---
            var boUser2 = new User
            {
                Id = Guid.Parse("a5b6c7d8-e9f0-4789-1234-56789abcdef6"),
                UserName = "BusinessOwner2",
                FullName = "Hoang Thi Hong",
                NormalizedUserName = "BUSINESSOWNER2",
                Organization = "VNPT Technology",
                IsApproved = true,
                IsActive = true,
                Email = "bo2@gmail.com",
                NormalizedEmail = "BO2@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
            boUser2.PasswordHash = hasher.HashPassword(boUser2, "1");

            // --- BUSINESS OWNER 3 ---
            var boUser3 = new User
            {
                Id = Guid.Parse("b6c7d8e9-f0a1-4789-1234-56789abcdef7"),
                UserName = "BusinessOwner3",
                FullName = "Le Van Phuc",
                NormalizedUserName = "BUSINESSOWNER3",
                Organization = "CMC Corporation",
                IsApproved = true,
                IsActive = true,
                Email = "bo3@gmail.com",
                NormalizedEmail = "BO3@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
            boUser3.PasswordHash = hasher.HashPassword(boUser3, "1");

            builder.HasData(
                adminUser,
                memberUser1,
                memberUser2,
                memberUser3,
                memberUser4,
                pmUser,
                boUser1,
                boUser2,
                boUser3
            );
        }
    }
}
