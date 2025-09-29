using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyCLB.Application.Entities;

namespace QuanLyCLB.Infrastructure.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.Address)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(b => b.GooglePlaceId)
            .HasMaxLength(200);

        builder.Property(b => b.GoogleMapsEmbedUrl)
            .HasMaxLength(500);

        builder.Property(b => b.AllowedRadiusMeters)
            .HasColumnType("float");

        builder.HasIndex(b => b.Name).IsUnique();
    }
}
