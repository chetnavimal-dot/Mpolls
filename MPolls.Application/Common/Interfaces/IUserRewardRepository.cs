using System.Collections.Generic;
using MPolls.Domain.Entities;

namespace MPolls.Application.Common.Interfaces;

public interface IUserRewardRepository
{
    Task<List<UserReward>> GetPanelistRewardsAsync(string panelistUlid);
}
