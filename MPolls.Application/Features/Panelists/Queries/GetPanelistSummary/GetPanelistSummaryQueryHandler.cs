using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MPolls.Application.Common.Interfaces;

namespace MPolls.Application.Features.Panelists.Queries.GetPanelistSummary;

public sealed class GetPanelistSummaryQueryHandler : IRequestHandler<GetPanelistSummaryQuery, PanelistSummary?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICountryRepository _countryRepository;
    private static readonly IReadOnlyList<KeyValuePair<int, string>> GenderOptions = new List<KeyValuePair<int, string>>
    {
        new(1, "Male"),
        new(2, "Female"),
        new(3, "Other"),
        new(4, "Prefer not to say"),
    };

    public GetPanelistSummaryQueryHandler(IApplicationDbContext dbContext, ICountryRepository countryRepository)
    {
        _dbContext = dbContext;
        _countryRepository = countryRepository;
    }

    public async Task<PanelistSummary?> Handle(GetPanelistSummaryQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirebaseId))
        {
            return null;
        }

        var firebaseId = request.FirebaseId.Trim();

        var panelist = await _dbContext.Panelists
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.FirebaseId == firebaseId, cancellationToken);
        
        

        return panelist is null
            ? null
            : new PanelistSummary(panelist.Id, panelist.FirebaseId, panelist.Email, panelist.Ulid, panelist.Verified, panelist.Onboarded, panelist.Age, panelist.Gender != null ? GenderOptions[panelist.Gender ?? 1].Value : null, _countryRepository.GetByCodeAsync(panelist.CountryCode ?? 0, cancellationToken)?.Result?.CountryName);
    }
}
