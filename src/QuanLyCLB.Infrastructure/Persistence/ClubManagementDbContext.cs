using Microsoft.EntityFrameworkCore;
using QuanLyCLB.Application.Entities;

namespace QuanLyCLB.Infrastructure.Persistence;

/// <summary>
/// DbContext chịu trách nhiệm ánh xạ giữa mô hình miền (Domain/Application) và cơ sở dữ liệu SQL Server.
/// Đây là cầu nối giữa tầng hạ tầng (Infrastructure) và tầng nghiệp vụ (Application).
/// </summary>
public class ClubManagementDbContext : DbContext
{
    public ClubManagementDbContext(DbContextOptions<ClubManagementDbContext> options)
        : base(options)
    {
    }

    // Khai báo các DbSet tương ứng với từng bảng trong cơ sở dữ liệu
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<TrainingClass> TrainingClasses => Set<TrainingClass>();
    public DbSet<ClassSchedule> ClassSchedules => Set<ClassSchedule>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<AttendanceTicket> AttendanceTickets => Set<AttendanceTicket>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<PayrollDetail> PayrollDetails => Set<PayrollDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Áp dụng toàn bộ cấu hình Fluent API trong assembly Infrastructure (tầng dữ liệu)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClubManagementDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
