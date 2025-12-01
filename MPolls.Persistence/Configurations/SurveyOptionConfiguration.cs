using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPolls.Domain.Entities;

namespace MPolls.Persistence.Configurations;

public class SurveyOptionConfiguration : IEntityTypeConfiguration<SurveyOption>
{
    public void Configure(EntityTypeBuilder<SurveyOption> builder)
    {
        builder.ToTable("SurveyOptions");

        builder.HasKey(option => option.OptionId);

        builder.Property(option => option.OptionText)
            .IsRequired();
    }
}
