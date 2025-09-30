using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyCLB.Application.Entities;

namespace QuanLyCLB.Infrastructure.Configurations;

public class PasswordResetOtpConfiguration : IEntityTypeConfiguration<PasswordResetOtp>
{
    public void Configure(EntityTypeBuilder<PasswordResetOtp> builder)
    {
        builder.ToTable("PasswordResetOtps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CodeHash).HasMaxLength(256).IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnType("datetime2");
        builder.Property(x => x.VerifiedAt).HasColumnType("datetime2");
        builder.Property(x => x.IsUsed).HasDefaultValue(false);

        builder.HasIndex(x => new { x.UserAccountId, x.IsUsed });

        builder.HasOne(x => x.User)
            .WithMany(x => x.PasswordResetOtps)
            .HasForeignKey(x => x.UserAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
