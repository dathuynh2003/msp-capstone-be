using MSP.Domain.Entities;
using MSP.Infrastructure.SeedConfiguration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace MSP.Infrastructure.Persistence.DBContext
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new RoleConfiguration());
            builder.ApplyConfiguration(new UserConfiguration());
            builder.Entity<User>()
                .Property(u => u.FirstName)
                .HasMaxLength(256);
            builder.Entity<User>()
                .Property(u => u.LastName)
            .HasMaxLength(256);

            builder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications", "notification");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.Data).HasMaxLength(4000);

                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsRead).IsRequired();

                // Indexes
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsRead);
            });
        }
    }
}
