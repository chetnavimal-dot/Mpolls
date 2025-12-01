using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MPolls.Domain.Entities;

namespace MPolls.Application.Common.Interfaces;

public interface ICountryRepository
{
    Task<IEnumerable<Country>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<Country?> GetByCodeAsync(int countryCode, CancellationToken cancellationToken = default);
}
