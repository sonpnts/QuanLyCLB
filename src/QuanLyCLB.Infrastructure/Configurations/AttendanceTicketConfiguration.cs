using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyCLB.Application.Entities;

namespace QuanLyCLB.Infrastructure.Configurations;

public class AttendanceTicketConfiguration : IEntityTypeConfiguration<AttendanceTicket>
{
    public void Configure(EntityTypeBuilder<AttendanceTicket> builder)
    {
        builder.ToTable("AttendanceTickets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(200).IsRequired();

        builder.HasOne(x => x.ClassSchedule)
            .WithMany()
            .HasForeignKey(x => x.ClassScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Coach)
            .WithMany()
            .HasForeignKey(x => x.CoachId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
