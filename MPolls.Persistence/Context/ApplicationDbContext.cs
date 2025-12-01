using Microsoft.EntityFrameworkCore;
using MPolls.Application.Common.Interfaces;
using MPolls.Domain.Entities;

namespace MPolls.Persistence.Context;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Panelist> Panelists => Set<Panelist>();

    public DbSet<SurveyCategory> SurveyCategories => Set<SurveyCategory>();

    public DbSet<SurveyQuestion> SurveyQuestions => Set<SurveyQuestion>();

    public DbSet<SurveyOption> SurveyOptions => Set<SurveyOption>();

    public DbSet<SurveyMatrixOption> SurveyMatrixOptions => Set<SurveyMatrixOption>();

    public DbSet<UserReward> UserRewards => Set<UserReward>();

    public DbSet<PanelistProfile> PanelistProfiles => Set<PanelistProfile>();

    public DbSet<Country> Countries => Set<Country>();

    public DbSet<RecommendedSurvey> RecommendedSurveys => Set<RecommendedSurvey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
