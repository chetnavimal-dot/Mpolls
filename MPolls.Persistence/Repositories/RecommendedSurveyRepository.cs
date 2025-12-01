using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MPolls.Application.Common.Interfaces;
using MPolls.Domain.Entities;
using MPolls.Persistence.Context;

namespace MPolls.Persistence.Repositories;

public class RecommendedSurveyRepository : IRecommendedSurveyRepository
{
    private readonly ApplicationDbContext _context;

    public RecommendedSurveyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RecommendedSurvey>> GetByPanelistIdAsync(string panelistId, bool includeCompleted, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(panelistId))
        {
            return Array.Empty<RecommendedSurvey>();
        }

        var trimmedPanelistId = panelistId.Trim();

        var query = _context.RecommendedSurveys
            .AsNoTracking()
            .Where(survey => survey.PanelistId == trimmedPanelistId);

        if (!includeCompleted)
        {
            query = query.Where(survey => survey.CompletedOn == null);
        }

        return await query
            .OrderBy(survey => survey.ExpiringOn ?? DateTime.MaxValue)
            .ThenBy(survey => survey.AssignedOn)
            .ToListAsync(cancellationToken);
    }

    public async Task<RecommendedSurvey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.RecommendedSurveys
            .FirstOrDefaultAsync(survey => survey.Id == id, cancellationToken);
    }

    public async Task AddAsync(RecommendedSurvey survey, CancellationToken cancellationToken = default)
    {
        await _context.RecommendedSurveys.AddAsync(survey, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(RecommendedSurvey survey, CancellationToken cancellationToken = default)
    {
        _context.RecommendedSurveys.Update(survey);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
