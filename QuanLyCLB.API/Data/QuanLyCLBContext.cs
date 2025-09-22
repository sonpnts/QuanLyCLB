using Microsoft.EntityFrameworkCore;
using QuanLyCLB.API.Models;

namespace QuanLyCLB.API.Data
{
    public class QuanLyCLBContext : DbContext
    {
        public QuanLyCLBContext(DbContextOptions<QuanLyCLBContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Schedule> Schedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.GoogleId).IsUnique();
                
                entity.Property(e => e.Role).HasConversion<int>();
            });

            // Student entity configuration
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                
                entity.Property(e => e.Status).HasConversion<int>();
            });

            // Class entity configuration
            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.FeePerMonth).HasColumnType("decimal(18,2)");
                
                // Configure relationships
                entity.HasOne(e => e.Trainer)
                    .WithMany(u => u.ClassesAsTrainer)
                    .HasForeignKey(e => e.TrainerId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Assistant)
                    .WithMany(u => u.ClassesAsAssistant)
                    .HasForeignKey(e => e.AssistantId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Enrollment entity configuration
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Status).HasConversion<int>();
                
                // Configure relationships
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Enrollments)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Class)
                    .WithMany(c => c.Enrollments)
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Unique constraint to prevent duplicate enrollments
                entity.HasIndex(e => new { e.StudentId, e.ClassId }).IsUnique();
            });

            // Attendance entity configuration
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Status).HasConversion<int>();
                
                // Configure relationships
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.AttendanceRecords)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Class)
                    .WithMany(c => c.AttendanceRecords)
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.RecordedBy)
                    .WithMany(u => u.AttendanceRecords)
                    .HasForeignKey(e => e.RecordedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Payment entity configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.PaymentType).HasConversion<int>();
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                
                // Configure relationships
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Payments)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Class)
                    .WithMany()
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Schedule entity configuration
            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.DayOfWeek).HasConversion<int>();
                
                // Configure relationships
                entity.HasOne(e => e.Class)
                    .WithMany(c => c.Schedules)
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Schedules)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}