using MediatR;
using MPolls.WebUI.Models;

namespace MPolls.Application.Features.UserRewards.Queries;

public record GetUserRewardsQuery(string PanelistUlid) : IRequest<RewardResponse>;

