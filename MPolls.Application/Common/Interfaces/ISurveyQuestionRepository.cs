using MPolls.Domain.Entities;

namespace MPolls.Application.Common.Interfaces;

public interface ISurveyQuestionRepository
{
    Task<IReadOnlyList<SurveyQuestion>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
}
