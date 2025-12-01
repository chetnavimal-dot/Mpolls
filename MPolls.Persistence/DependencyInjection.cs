using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MPolls.Application.Common.Interfaces;
using MPolls.Persistence.Context;
using MPolls.Persistence.Repositories;

namespace MPolls.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ISurveyCategoryRepository, SurveyCategoryRepository>();
        services.AddScoped<ISurveyQuestionRepository, SurveyQuestionRepository>();
        services.AddScoped<IUserRewardRepository, UserRewardRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IRecommendedSurveyRepository, RecommendedSurveyRepository>();

        return services;
    }
}
