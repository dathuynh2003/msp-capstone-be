using MSP.Domain.Entities;
using MSP.Infrastructure.SeedConfiguration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using MSP.Shared.Enums;

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
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Milestone> Milestones { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Todo> Todos { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Limitation> Limitations { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<OrganizationInvitation> OrganizationInvitations { get; set; }
        public DbSet<TaskHistory> TaskHistories { get; set; }




        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new RoleConfiguration());
            builder.ApplyConfiguration(new UserConfiguration());
            builder.ApplyConfiguration(new UserRoleConfiguration());
            builder.Entity<User>()
                .Property(u => u.FullName)
                .HasMaxLength(256);

            builder.Entity<User>()
                .HasOne(u => u.ManagedBy)
                .WithMany(u => u.ManagedUsers)
                .HasForeignKey(u => u.ManagedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Package - Feature (many-to-many)
            builder.Entity<Package>()
                .HasMany(p => p.Limitations)
                .WithMany(f => f.Packages)
                .UsingEntity<Dictionary<string, object>>(
                    "PackageLimitation",
                    j => j.HasOne<Limitation>().WithMany().HasForeignKey("LimitationId").OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<Package>().WithMany().HasForeignKey("PackageId").OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("PackageId", "LimitationId");
                        j.ToTable("PackageLimitations");
                    });

            // Subscription
            builder.Entity<Subscription>(entity =>
            {
                entity.HasOne(s => s.Package)
                    .WithMany()
                    .HasForeignKey(s => s.PackageId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Project
            builder.Entity<Project>(entity =>
            {
                entity.HasOne(p => p.CreatedBy)
                    .WithMany()
                    .HasForeignKey(p => p.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Owner)
                    .WithMany()
                    .HasForeignKey(p => p.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(p => p.ProjectMembers)
                    .WithOne(pm => pm.Project)
                    .HasForeignKey(pm => pm.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Milestones)
                    .WithOne(m => m.Project)
                    .HasForeignKey(m => m.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.ProjectTasks)
                    .WithOne(t => t.Project)
                    .HasForeignKey(t => t.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Documents)
                    .WithOne(d => d.Project)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Meetings)
                    .WithOne(m => m.Project)
                    .HasForeignKey(m => m.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ProjectMember
            builder.Entity<ProjectMember>(entity =>
            {
                entity.HasOne(pm => pm.Member)
                    .WithMany()
                    .HasForeignKey(pm => pm.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Document
            builder.Entity<Document>(entity =>
            {
                entity.HasOne(d => d.Owner)
                    .WithMany()
                    .HasForeignKey(d => d.OwnerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Milestone
            builder.Entity<Milestone>(entity =>
            {
                entity.HasOne(m => m.User)
                    .WithMany()
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(m => m.ProjectTasks)
                    .WithMany(t => t.Milestones)  // thiết kế m:n
                    .UsingEntity<Dictionary<string, object>>(
                        "MilestoneTask",
                        j => j.HasOne<ProjectTask>().WithMany().HasForeignKey("ProjectTaskId").OnDelete(DeleteBehavior.Cascade),
                        j => j.HasOne<Milestone>().WithMany().HasForeignKey("MilestoneId").OnDelete(DeleteBehavior.Cascade),
                        j =>
                        {
                            j.HasKey("MilestoneId", "ProjectTaskId");
                            j.ToTable("MilestoneTasks");
                        });
            });

            // Task
            builder.Entity<ProjectTask>(entity =>
            {
                entity.HasOne(t => t.User)
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Todo)
                    .WithMany(todo => todo.ProjectTasks)
                    .HasForeignKey(t => t.TodoId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(t => t.Comments)
                    .WithOne(c => c.ProjectTask)
                    .HasForeignKey(c => c.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Comment
            builder.Entity<Comment>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Meeting
            builder.Entity<Meeting>(entity =>
            {
                entity.HasOne(m => m.CreatedBy)
                    .WithMany()
                    .HasForeignKey(m => m.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Milestone)
                    .WithMany()
                    .HasForeignKey(m => m.MilestoneId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(m => m.Attendees)
                    .WithMany()
                    .UsingEntity<Dictionary<string, object>>(
                        "MeetingUser",
                        j => j.HasOne<User>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade),
                        j => j.HasOne<Meeting>().WithMany().HasForeignKey("MeetingId").OnDelete(DeleteBehavior.Cascade),
                        j =>
                        {
                            j.HasKey("MeetingId", "UserId");
                            j.ToTable("MeetingUsers");
                        });
            });

            // Todo
            builder.Entity<Todo>(entity =>
            {
                entity.HasOne(t => t.Meeting)
                    .WithMany(m => m.Todos)
                    .HasForeignKey(t => t.MeetingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.User)
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(t => t.ReferencedTasks)
                    .WithMany(t => t.ReferencingTodos);

                //Global Query Filter - Tự động loại trừ deleted todos
                entity.HasQueryFilter(t => t.Status != TodoStatus.Deleted);
            });

            // OrganizationInvitation
            builder.Entity<OrganizationInvitation>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Id);

                // BusinessOwner relationship
                entity.HasOne(e => e.BusinessOwner)
                    .WithMany(u => u.OrganizationInvitationsAsOwner)
                    .HasForeignKey(e => e.BusinessOwnerId)
                    .OnDelete(DeleteBehavior.Restrict); // Tránh cascade delete conflict

                // Member relationship
                entity.HasOne(e => e.Member)
                    .WithMany(u => u.OrganizationInvitationsAsMember)
                    .HasForeignKey(e => e.MemberId)
                    .OnDelete(DeleteBehavior.Restrict); // Tránh cascade delete conflict

                // Indexes for better query performance
                entity.HasIndex(e => new { e.BusinessOwnerId, e.Status });
                entity.HasIndex(e => new { e.MemberId, e.Status });
                entity.HasIndex(e => e.Type);
            });

            // TaskHistory
            builder.Entity<TaskHistory>(entity =>
            {
                entity.HasOne(h => h.Task)
                    .WithMany(t => t.TaskHistories)
                    .HasForeignKey(h => h.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(h => h.FromUser)
                    .WithMany()
                    .HasForeignKey(h => h.FromUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(h => h.ToUser)
                    .WithMany()
                    .HasForeignKey(h => h.ToUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
