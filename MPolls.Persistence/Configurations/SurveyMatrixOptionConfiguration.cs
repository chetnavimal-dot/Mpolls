using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPolls.Domain.Entities;

namespace MPolls.Persistence.Configurations;

public class SurveyMatrixOptionConfiguration : IEntityTypeConfiguration<SurveyMatrixOption>
{
    public void Configure(EntityTypeBuilder<SurveyMatrixOption> builder)
    {
        builder.ToTable("SurveyMatrixOptions");

        builder.HasKey(option => option.MatrixRowId);

        builder.Property(option => option.MatrixRowText)
            .IsRequired();
    }
}
