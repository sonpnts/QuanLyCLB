using Microsoft.EntityFrameworkCore;
using QuanLyClb.Domain.Entities;

namespace QuanLyClb.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<TrainingClass> Classes => Set<TrainingClass>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<ClassSchedule> ClassSchedules => Set<ClassSchedule>();
    public DbSet<AttendanceSession> AttendanceSessions => Set<AttendanceSession>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<TuitionPayment> TuitionPayments => Set<TuitionPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.FullName).HasMaxLength(200);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasIndex(s => s.Email).IsUnique();
            entity.Property(s => s.FullName).HasMaxLength(200);
            entity.HasOne(s => s.CurrentClass)
                .WithMany()
                .HasForeignKey(s => s.CurrentClassId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TrainingClass>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(200);
            entity.HasOne(c => c.Coach)
                .WithMany(u => u.CoachingClasses)
                .HasForeignKey(c => c.CoachId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(c => c.Assistant)
                .WithMany(u => u.AssistingClasses)
                .HasForeignKey(c => c.AssistantId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(c => c.ParentClass)
                .WithMany(c => c.ClonedClasses)
                .HasForeignKey(c => c.ParentClassId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasIndex(e => new { e.StudentId, e.ClassId }).IsUnique();
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId);
            entity.HasOne(e => e.Class)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.ClassId);
        });

        modelBuilder.Entity<ClassSchedule>(entity =>
        {
            entity.HasOne(s => s.Class)
                .WithMany(c => c.Schedules)
                .HasForeignKey(s => s.ClassId);
        });

        modelBuilder.Entity<AttendanceSession>(entity =>
        {
            entity.HasOne(s => s.Class)
                .WithMany(c => c.AttendanceSessions)
                .HasForeignKey(s => s.ClassId);
            entity.HasMany(s => s.Records)
                .WithOne(r => r.Session)
                .HasForeignKey(r => r.AttendanceSessionId);
        });

        modelBuilder.Entity<TuitionPayment>(entity =>
        {
            entity.HasOne(p => p.Student)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.StudentId);
            entity.HasOne(p => p.Class)
                .WithMany()
                .HasForeignKey(p => p.ClassId);
            entity.HasOne(p => p.CollectedBy)
                .WithMany(u => u.CollectedPayments)
                .HasForeignKey(p => p.CollectedById)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
