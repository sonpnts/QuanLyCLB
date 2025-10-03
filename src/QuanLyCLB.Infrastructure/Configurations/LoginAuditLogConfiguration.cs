using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyCLB.Application.Entities;

namespace QuanLyCLB.Infrastructure.Configurations;

public class LoginAuditLogConfiguration : IEntityTypeConfiguration<LoginAuditLog>
{
    public void Configure(EntityTypeBuilder<LoginAuditLog> builder)
    {
        builder.ToTable("LoginAuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ApiEndpoint)
            .IsRequired()
            .HasMaxLength(400);

        builder.Property(x => x.LocationAddress)
            .HasMaxLength(500);

        builder.Property(x => x.DeviceInfo)
            .HasMaxLength(500);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(100);

        builder.Property(x => x.Message)
            .HasMaxLength(500);

        builder.HasOne(x => x.UserAccount)
            .WithMany()
            .HasForeignKey(x => x.UserAccountId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
