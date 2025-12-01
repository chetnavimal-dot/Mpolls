using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPolls.Domain.Entities;

namespace MPolls.Persistence.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(country => country.CountryCode);

        builder.Property(country => country.CountryName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(country => country.CountryShortCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(country => country.IsActive)
            .IsRequired();
    }
}
