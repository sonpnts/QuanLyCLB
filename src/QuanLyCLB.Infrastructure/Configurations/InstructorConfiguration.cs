using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyCLB.Application.Entities;

namespace QuanLyCLB.Infrastructure.Configurations;

public class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
{
    public void Configure(EntityTypeBuilder<Instructor> builder)
    {
        builder.ToTable("Instructors");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.HourlyRate).HasPrecision(18, 2);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.HasIndex(x => x.UserAccountId).IsUnique();

        builder.HasOne(x => x.User)
            .WithOne(x => x.Instructor)
            .HasForeignKey<Instructor>(x => x.UserAccountId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
