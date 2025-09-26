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
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(50);
        builder.Property(x => x.HourlyRate).HasPrecision(18, 2);
        builder.HasIndex(x => x.Email).IsUnique();

        builder.HasOne(x => x.UserAccount)
            .WithOne(x => x.Instructor)
            .HasForeignKey<Instructor>(x => x.UserAccountId)
            .OnDelete(DeleteBehavior.SetNull);

    }
}
