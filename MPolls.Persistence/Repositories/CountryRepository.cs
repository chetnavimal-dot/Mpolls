using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MPolls.Application.Common.Interfaces;
using MPolls.Domain.Entities;

namespace MPolls.Persistence.Repositories;

public class CountryRepository : ICountryRepository
{
    private readonly IApplicationDbContext _context;

    public CountryRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Country>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Countries
            .AsNoTracking()
            .Where(country => country.IsActive)
            .OrderBy(country => country.CountryName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Country?> GetByCodeAsync(int countryCode, CancellationToken cancellationToken = default)
    {
        return await _context.Countries
            .AsNoTracking()
            .FirstOrDefaultAsync(country => country.CountryCode == countryCode, cancellationToken);
    }
}
