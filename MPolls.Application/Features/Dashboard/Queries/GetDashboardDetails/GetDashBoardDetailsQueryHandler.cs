using MediatR;
using MPolls.Application.Common.Interfaces;
using MPolls.Application.DTOs;
using MPolls.Application.DTOs.Surveys;
using MPolls.Domain.Enums;

namespace MPolls.Application.Features.Dashboard.Queries;

public class GetDashBoardDetailsQueryHandler : IRequestHandler<GetDashboardDetailsQuery, DashboardResponse>
{
    private readonly IUserRewardRepository _userRewardRepository;
    private readonly ISurveyCategoryRepository _surveyCategoryRepository;
    private readonly IRecommendedSurveyRepository _recommendedSurveyRepository;
    
    public GetDashBoardDetailsQueryHandler(IUserRewardRepository userRewardRepository, ISurveyCategoryRepository surveyCategoryRepository, IRecommendedSurveyRepository recommendedSurveyRepository)
    {
        _userRewardRepository = userRewardRepository;
        _surveyCategoryRepository = surveyCategoryRepository;
        _recommendedSurveyRepository = recommendedSurveyRepository;
    }

    public async Task<DashboardResponse> Handle(GetDashboardDetailsQuery request, CancellationToken cancellationToken)
    {
        var rewards = await _userRewardRepository.GetPanelistRewardsAsync(request.PanelistUlid);
        
        var totalEarned = rewards
            .Where(r => r.TransactionType == RewardTransactionType.Earned)
            .Sum(r => r.Points);

        var totalRedeemed = rewards
            .Where(r => r.TransactionType == RewardTransactionType.Redeemed)
            .Sum(r => Math.Abs(r.Points));

        var totalAvailable = totalEarned - totalRedeemed;
        
        var categories = await _surveyCategoryRepository.GetActiveAsync(cancellationToken);

        var categoriesWithQuestions = categories
            .Where(category => category.RewardPoints > 0)
            .Select(category => new SurveyCategoryDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Icon = category.Icon,
                IsActive = category.IsActive,
                RewardPoints = category.RewardPoints,
                RetakePoints = category.RetakePoints,
                RetakePointsIssueFrequency = category.RetakePointsIssueFrequency
            })
            .ToList();
        
        var uniqueCategoryCount = rewards
            .Where(r => r.CategoryId.HasValue)
            .Select(r => r.CategoryId!.Value)
            .Distinct()
            .Count();

        var profileStrength = (int) Math.Round((uniqueCategoryCount / (double) categoriesWithQuestions.Count) * 100);
        
        var currentMonth = DateTime.UtcNow.Month;
        var currentYear = DateTime.UtcNow.Year;

        var surveysTakenThisMonth = rewards
            .Where(r => 
                r.TransactionType == RewardTransactionType.Earned &&
                r.CreatedOn.Month == currentMonth &&
                r.CreatedOn.Year == currentYear)
            .Select(r => r.CategoryId)
            .Where(id => id.HasValue)
            .Distinct()
            .Count();
        
        var surveys = await _recommendedSurveyRepository.GetByPanelistIdAsync(request.PanelistUlid, false, cancellationToken);
        
        var now = DateTime.UtcNow;
        var activeSurveys = surveys
            .Where(s => s.ExpiringOn == null || s.ExpiringOn >= now)
            .ToList();
        
        var activeSurveysDtos = activeSurveys
            .Select(RecommendedSurveyDto.FromEntity)
            .ToList();
        
        return new DashboardResponse
        {
            ActiveSurveys = activeSurveysDtos,
            RewardBalance = totalAvailable,
            ProfileStrength = profileStrength,
            MonthlySurveys = surveysTakenThisMonth
        };
    }
}