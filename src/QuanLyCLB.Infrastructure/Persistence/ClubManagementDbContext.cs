using Microsoft.EntityFrameworkCore;
using QuanLyCLB.Application.Entities;

namespace QuanLyCLB.Infrastructure.Persistence;

public class ClubManagementDbContext : DbContext
{
    public ClubManagementDbContext(DbContextOptions<ClubManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<TrainingClass> TrainingClasses => Set<TrainingClass>();
    public DbSet<ClassSchedule> ClassSchedules => Set<ClassSchedule>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<AttendanceTicket> AttendanceTickets => Set<AttendanceTicket>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<PayrollDetail> PayrollDetails => Set<PayrollDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClubManagementDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
