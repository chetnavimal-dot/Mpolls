using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPolls.Domain.Entities;

namespace MPolls.Persistence.Configurations;

public class SurveyQuestionConfiguration : IEntityTypeConfiguration<SurveyQuestion>
{
    public void Configure(EntityTypeBuilder<SurveyQuestion> builder)
    {
        builder.ToTable("SurveyQuestions");

        builder.HasKey(question => question.QuestionId);

        builder.Property(question => question.QuestionText)
            .IsRequired();

        builder.Property(question => question.CategoryId)
            .IsRequired();

        builder.Property(question => question.ResponseType)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(question => question.QuestionType)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(question => question.CountryCode)
            .HasMaxLength(10);

        builder.HasMany(question => question.Options)
            .WithOne(option => option.Question)
            .HasForeignKey(option => option.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(question => question.MatrixOptions)
            .WithOne(option => option.Question)
            .HasForeignKey(option => option.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
