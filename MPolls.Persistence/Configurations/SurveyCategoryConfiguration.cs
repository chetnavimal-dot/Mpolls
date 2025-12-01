using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPolls.Domain.Entities;

namespace MPolls.Persistence.Configurations;

public class SurveyCategoryConfiguration : IEntityTypeConfiguration<SurveyCategory>
{
    public void Configure(EntityTypeBuilder<SurveyCategory> builder)
    {
        builder.ToTable("SurveyCategories");

        builder.HasKey(category => category.CategoryId);

        builder.Property(category => category.CategoryId)
            .ValueGeneratedNever();

        builder.Property(category => category.CategoryName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(category => category.Icon)
            .HasMaxLength(200);

        builder.Property(category => category.IsActive)
            .HasDefaultValue(true);

        builder.Property(category => category.RewardPoints)
            .HasDefaultValue(0);

        builder.Property(category => category.RetakePoints)
            .HasDefaultValue(0);

        builder.Property(category => category.RetakePointsIssueFrequency)
            .HasDefaultValue(0);
    }
}
