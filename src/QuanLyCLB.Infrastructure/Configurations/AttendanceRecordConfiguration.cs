using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyCLB.Application.Entities;
using QuanLyCLB.Application.Enums;

namespace QuanLyCLB.Infrastructure.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(AttendanceStatus.Pending);

        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasOne(x => x.ClassSchedule)
            .WithMany(s => s.AttendanceRecords)
            .HasForeignKey(x => x.ClassScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Instructor)
            .WithMany(i => i.AttendanceRecords)
            .HasForeignKey(x => x.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Ticket)
            .WithOne(t => t.AttendanceRecord)
            .HasForeignKey<AttendanceRecord>(x => x.TicketId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
