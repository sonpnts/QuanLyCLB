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
        builder.HasIndex(x => new { x.TrainingClassId, x.StudyDate }).IsUnique();

        builder.Property(x => x.LocationName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.AllowedRadiusMeters).HasPrecision(18, 2);

        builder.HasOne(x => x.TrainingClass)
            .WithMany(c => c.Schedules)
            .HasForeignKey(x => x.TrainingClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
