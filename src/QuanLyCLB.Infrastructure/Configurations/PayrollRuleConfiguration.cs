using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyCLB.Application.Entities;

namespace QuanLyCLB.Infrastructure.Configurations;

public class PayrollRuleConfiguration : IEntityTypeConfiguration<PayrollRule>
{
    public void Configure(EntityTypeBuilder<PayrollRule> builder)
    {
        builder.ToTable("PayrollRules");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RoleName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SkillLevel).HasMaxLength(200).IsRequired();
        builder.Property(x => x.HourlyRate).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasIndex(x => new { x.RoleName, x.SkillLevel }).IsUnique();
    }
}
