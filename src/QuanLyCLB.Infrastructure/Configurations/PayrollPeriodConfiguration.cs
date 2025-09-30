using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyCLB.Application.Entities;

namespace QuanLyCLB.Infrastructure.Configurations;

public class PayrollPeriodConfiguration : IEntityTypeConfiguration<PayrollPeriod>
{
    public void Configure(EntityTypeBuilder<PayrollPeriod> builder)
    {
        builder.ToTable("PayrollPeriods");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.CoachId, x.Year, x.Month }).IsUnique();
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalHours).HasPrecision(18, 2);

        builder.HasOne(x => x.Coach)
            .WithMany()
            .HasForeignKey(x => x.CoachId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PayrollDetailConfiguration : IEntityTypeConfiguration<PayrollDetail>
{
    public void Configure(EntityTypeBuilder<PayrollDetail> builder)
    {
        builder.ToTable("PayrollDetails");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Hours).HasPrecision(18, 2);
        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.HasOne(x => x.PayrollPeriod)
            .WithMany(p => p.Details)
            .HasForeignKey(x => x.PayrollPeriodId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AttendanceRecord)
            .WithMany()
            .HasForeignKey(x => x.AttendanceRecordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
