using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyCLB.Application.Entities;

namespace QuanLyCLB.Infrastructure.Configurations;

public class ClassAssistantAssignmentConfiguration : IEntityTypeConfiguration<ClassAssistantAssignment>
{
    public void Configure(EntityTypeBuilder<ClassAssistantAssignment> builder)
    {
        builder.ToTable("ClassAssistantAssignments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RoleName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne(x => x.TrainingClass)
            .WithMany(x => x.AssistantAssignments)
            .HasForeignKey(x => x.TrainingClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ClassSchedule)
            .WithMany()
            .HasForeignKey(x => x.ClassScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Assistant)
            .WithMany()
            .HasForeignKey(x => x.AssistantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
