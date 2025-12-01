using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPolls.Domain.Entities;

namespace MPolls.Persistence.Configurations;

public class PanelistConfiguration : IEntityTypeConfiguration<Panelist>
{
    public void Configure(EntityTypeBuilder<Panelist> builder)
    {
        builder.ToTable("Panelists");

        builder.HasKey(panelist => panelist.Id);

        builder.Property(panelist => panelist.Id)
            .ValueGeneratedOnAdd();

        builder.Property(panelist => panelist.Ulid)
            .IsRequired()
            .HasMaxLength(26)
            .HasColumnType("char(26)")
            .ValueGeneratedNever();

        builder.HasIndex(panelist => panelist.Ulid)
            .IsUnique();

        builder.Property(panelist => panelist.FirebaseId)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(panelist => panelist.FirebaseId)
            .IsUnique();

        builder.Property(panelist => panelist.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(panelist => panelist.CreatedDate)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        builder.Property(panelist => panelist.Verified)
            .HasDefaultValue(false);

        builder.Property(panelist => panelist.CountryCode);

        builder.Property(panelist => panelist.Gender);

        builder.Property(panelist => panelist.Age);

        builder.Property(panelist => panelist.ConsentCollectedOn)
            .HasColumnType("datetime2(3)");

        builder.Property(panelist => panelist.Onboarded)
            .HasDefaultValue(false);
    }
}
