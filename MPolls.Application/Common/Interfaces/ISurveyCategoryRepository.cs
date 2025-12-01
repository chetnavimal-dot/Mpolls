using MPolls.Domain.Entities;

namespace MPolls.Application.Common.Interfaces;

public interface ISurveyCategoryRepository
{
    Task<IEnumerable<SurveyCategory>> GetActiveAsync(CancellationToken cancellationToken = default);
}
