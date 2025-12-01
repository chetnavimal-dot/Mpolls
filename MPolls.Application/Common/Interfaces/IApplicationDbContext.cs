using Microsoft.EntityFrameworkCore;
using MPolls.Domain.Entities;

namespace MPolls.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Employee> Employees { get; }

    DbSet<Panelist> Panelists { get; }

    DbSet<SurveyCategory> SurveyCategories { get; }

    DbSet<SurveyQuestion> SurveyQuestions { get; }

    DbSet<SurveyOption> SurveyOptions { get; }

    DbSet<SurveyMatrixOption> SurveyMatrixOptions { get; }

    DbSet<UserReward> UserRewards { get; }

    DbSet<PanelistProfile> PanelistProfiles { get; }

    DbSet<Country> Countries { get; }

    DbSet<RecommendedSurvey> RecommendedSurveys { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
