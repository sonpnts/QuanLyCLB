using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyCLB.Application.Entities;

namespace QuanLyCLB.Infrastructure.Configurations;

public class ClassScheduleConfiguration : IEntityTypeConfiguration<ClassSchedule>
{
    public void Configure(EntityTypeBuilder<ClassSchedule> builder)
    {
        builder.ToTable("ClassSchedules");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TrainingClassId, x.DayOfWeek }).IsUnique();
        builder.HasIndex(x => new { x.TrainingClassId, x.DayOfWeek, x.StartTime, x.EndTime }).IsUnique();

        builder.HasOne(x => x.TrainingClass)
            .WithMany(c => c.Schedules)
            .HasForeignKey(x => x.TrainingClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Branch)
            .WithMany(b => b.ClassSchedules)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
