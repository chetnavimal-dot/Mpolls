using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MPolls.Domain.Entities;

namespace MPolls.Application.Common.Interfaces;

public interface IRecommendedSurveyRepository
{
    Task<IReadOnlyList<RecommendedSurvey>> GetByPanelistIdAsync(string panelistId, bool includeCompleted, CancellationToken cancellationToken = default);

    Task<RecommendedSurvey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(RecommendedSurvey survey, CancellationToken cancellationToken = default);

    Task UpdateAsync(RecommendedSurvey survey, CancellationToken cancellationToken = default);
}
