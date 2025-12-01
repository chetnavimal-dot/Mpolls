using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPolls.Domain.Entities;

namespace MPolls.Persistence.Configurations;

public class PanelistProfileConfiguration : IEntityTypeConfiguration<PanelistProfile>
{
    public void Configure(EntityTypeBuilder<PanelistProfile> builder)
    {
        builder.ToTable("PanelistProfiles");

        builder.HasKey(profile => profile.ResponseId);

        builder.Property(profile => profile.ResponseId)
            .ValueGeneratedNever();

        builder.Property(profile => profile.PanelistId)
            .IsRequired()
            .HasMaxLength(26)
            .IsUnicode(false)
            .HasColumnType("char(26)");


        builder.Property(profile => profile.CategoryId)
            .IsRequired();

        builder.Property(profile => profile.QuestionId)
            .IsRequired();

        builder.Property(profile => profile.MatrixQuestionId)
            .IsRequired(false);

        builder.Property(profile => profile.AnswerIds)
            .HasMaxLength(4000)
            .IsUnicode();

        builder.Property(profile => profile.Text)
            .IsUnicode();

        builder.Property(profile => profile.Numeric)
            .HasColumnType("decimal(18,4)");

        builder.Property(profile => profile.DateTime)
            .HasColumnType("datetime2(3)");

        builder.Property(profile => profile.CreatedOn)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(profile => profile.Panelist)
            .WithMany()
            .HasForeignKey(profile => profile.PanelistId)
            .HasPrincipalKey(panelist => panelist.Ulid)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
