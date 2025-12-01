using System.Linq;
using Microsoft.EntityFrameworkCore;
using MPolls.Application.Common.Interfaces;
using MPolls.Domain.Entities;

namespace MPolls.Persistence.Repositories;

public class SurveyQuestionRepository : ISurveyQuestionRepository
{
    private readonly IApplicationDbContext _context;

    public SurveyQuestionRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SurveyQuestion>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.SurveyQuestions
            .Include(question => question.Options)
            .Include(question => question.MatrixOptions)
            .Where(question => question.CategoryId == categoryId)
            .OrderBy(question => question.QuestionId)
            .ToListAsync(cancellationToken);
    }
}
