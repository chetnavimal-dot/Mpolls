using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPolls.Domain.Entities;

namespace MPolls.Persistence.Configurations;

public class RecommendedSurveyConfiguration : IEntityTypeConfiguration<RecommendedSurvey>
{
    public void Configure(EntityTypeBuilder<RecommendedSurvey> builder)
    {
        builder.ToTable("RecommendedSurveys");

        builder.HasKey(survey => survey.Id);

        builder.Property(survey => survey.Id)
            .ValueGeneratedOnAdd();

        builder.Property(survey => survey.SurveyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(survey => survey.SurveyDescription)
            .HasColumnType("nvarchar(max)");

        builder.Property(survey => survey.SurveyLink)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(survey => survey.ExpiringOn)
            .HasColumnType("datetime2(3)");

        builder.Property(survey => survey.EstimatedRewardPoints)
            .IsRequired();

        builder.Property(survey => survey.MultipleResponseAllowed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(survey => survey.AssignedOn)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(survey => survey.CompletedOn)
            .HasColumnType("datetime2(3)");

        builder.Property(survey => survey.PanelistId)
            .IsRequired()
            .HasMaxLength(26)
            .HasColumnType("char(26)");

        builder.HasIndex(survey => survey.PanelistId);

        builder.HasOne<Panelist>()
            .WithMany()
            .HasForeignKey(survey => survey.PanelistId)
            .HasPrincipalKey(panelist => panelist.Ulid)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
