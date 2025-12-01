using System;
using System.Linq;
using MediatR;
using MPolls.Application.Common.Interfaces;
using MPolls.Domain.Enums;
using MPolls.WebUI.Models;

namespace MPolls.Application.Features.UserRewards.Queries;

public class GetUserRewardsQueryHandler : IRequestHandler<GetUserRewardsQuery, RewardResponse>
{
    private readonly IUserRewardRepository _userRewardRepository;
    private readonly ISurveyCategoryRepository _surveyCategoryRepository;

    public GetUserRewardsQueryHandler(IUserRewardRepository userRewardRepository, ISurveyCategoryRepository surveyCategoryRepository)
    {
        _userRewardRepository = userRewardRepository;
        _surveyCategoryRepository = surveyCategoryRepository;
    }

    public async Task<RewardResponse> Handle(GetUserRewardsQuery request, CancellationToken cancellationToken)
    {
        var rewards = await _userRewardRepository.GetPanelistRewardsAsync(request.PanelistUlid);
        var categories = await _surveyCategoryRepository.GetActiveAsync();

        var expirationCutoff = DateTime.UtcNow.AddMonths(-2);

        var earnedRewards = rewards
            .Where(r => r.TransactionType == RewardTransactionType.Earned)
            .ToList();

        var totalEarned = earnedRewards.Sum(r => r.Points);
        var totalExpired = earnedRewards
            .Where(r => r.CreatedOn < expirationCutoff)
            .Sum(r => r.Points);
        var totalActiveEarned = totalEarned - totalExpired;

        var totalRedeemed = rewards
            .Where(r => r.TransactionType == RewardTransactionType.Redeemed)
            .Sum(r => Math.Abs(r.Points));

        var totalAvailable = Math.Max(0, totalActiveEarned - totalRedeemed);

        var entries = (from r in rewards
            join c in categories on r.CategoryId equals c.CategoryId
            select new RewardEntry
            {
                Description = r.Description,
                CompletedOn = r.CreatedOn,
                Points = r.Points,
                IsExpired = r.TransactionType == RewardTransactionType.Earned && r.CreatedOn < expirationCutoff
            }).ToArray();

        return new RewardResponse
        {
            TotalRewardEarned = totalEarned,
            TotalRewardRedeemed = totalRedeemed,
            TotalRewardAvailable = totalAvailable,
            TotalRewardExpired = totalExpired,
            TotalRewardClaimed = totalRedeemed,
            RewardEntries = entries
        };
    }
}
