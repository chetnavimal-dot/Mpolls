using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MPolls.Application.Common.Interfaces;

namespace MPolls.Application.Features.Panelists.Queries.GetPanelistUlid;

public sealed class GetPanelistUlidQueryHandler : IRequestHandler<GetPanelistUlidQuery, string?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPanelistUlidQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string?> Handle(GetPanelistUlidQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirebaseId))
        {
            return null;
        }

        var firebaseId = request.FirebaseId.Trim();

        var panelist = await _dbContext.Panelists
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.FirebaseId == firebaseId, cancellationToken);

        return panelist?.Ulid;
    }
}
