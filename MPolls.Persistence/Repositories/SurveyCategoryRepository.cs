using System.Linq;
using Microsoft.EntityFrameworkCore;
using MPolls.Application.Common.Interfaces;
using MPolls.Domain.Entities;

namespace MPolls.Persistence.Repositories;

public class SurveyCategoryRepository : ISurveyCategoryRepository
{
    private readonly IApplicationDbContext _context;

    public SurveyCategoryRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SurveyCategory>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SurveyCategories
            .Where(category => category.IsActive)
            .OrderBy(category => category.CategoryName)
            .ToListAsync(cancellationToken);
    }
}
