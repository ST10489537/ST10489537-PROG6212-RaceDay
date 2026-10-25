
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;



namespace RaceDay.Api.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options) { }

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<Result> Results => Set<Result>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraints
            modelBuilder.Entity<Role>().HasIndex(r => r.RoleName).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => new { e.ParticipantID, e.EventID }).IsUnique();
            modelBuilder.Entity<Result>().HasIndex(r => r.EnrolmentID).IsUnique();

            // Relationships (Restrict avoids SQL Server "multiple cascade paths" errors)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role).WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser).WithMany(u => u.OrganisedEvents)
                .HasForeignKey(e => e.OrganiserID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event).WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventID).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Participant).WithMany(u => u.Enrolments)
                .HasForeignKey(e => e.ParticipantID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Event).WithMany(ev => ev.Enrolments)
                .HasForeignKey(e => e.EventID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Category).WithMany(c => c.Enrolments)
                .HasForeignKey(e => e.CategoryID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Result>()
                .HasOne(r => r.Enrolment).WithOne(e => e.Result)
                .HasForeignKey<Result>(r => r.EnrolmentID).OnDelete(DeleteBehavior.Cascade);

            // Enums stored as text, matching your SQL script's CHECK values
            modelBuilder.Entity<Event>()
                .Property(e => e.EventType).HasConversion<string>().HasMaxLength(10);
            modelBuilder.Entity<Enrolment>()
                .Property(e => e.Status).HasConversion<string>().HasMaxLength(20)
                .HasDefaultValue(EnrolmentStatus.Pending);

            // Precision and defaults
            modelBuilder.Entity<Event>().Property(e => e.DistanceKm).HasPrecision(5, 2);
            modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Event>().Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Enrolment>().Property(e => e.EnrolmentDate).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Result>().Property(r => r.CapturedAt).HasDefaultValueSql("GETDATE()");

            // Seed the two roles (constant data, no passwords involved)
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleID = 1, RoleName = "Organiser" },
                new Role { RoleID = 2, RoleName = "Participant" });
        }
    }
}

