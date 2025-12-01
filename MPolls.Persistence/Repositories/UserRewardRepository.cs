using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MPolls.Application.Common.Interfaces;
using MPolls.Domain.Entities;
using MPolls.Persistence.Context;

namespace MPolls.Persistence.Repositories;

public class UserRewardRepository : IUserRewardRepository
{
    private readonly ApplicationDbContext _context;

    public UserRewardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserReward>> GetPanelistRewardsAsync(string panelistUlid)
    {
        return await _context.UserRewards
            .Where(r => r.PanelistUlid == panelistUlid)
            .ToListAsync();
    }
}
