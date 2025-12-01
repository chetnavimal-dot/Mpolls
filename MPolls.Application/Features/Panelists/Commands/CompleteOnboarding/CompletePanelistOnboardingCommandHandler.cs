using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MPolls.Application.Common.Interfaces;

namespace MPolls.Application.Features.Panelists.Commands.CompleteOnboarding;

public sealed class CompletePanelistOnboardingCommandHandler : IRequestHandler<CompletePanelistOnboardingCommand, bool>
{
    private readonly IApplicationDbContext _dbContext;

    public CompletePanelistOnboardingCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(CompletePanelistOnboardingCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirebaseId))
        {
            return false;
        }

        if (request.Age <= 0 || request.Gender <= 0 || request.CountryCode <= 0)
        {
            return false;
        }

        var firebaseId = request.FirebaseId.Trim();

        var countryIsActive = await _dbContext.Countries
            .AsNoTracking()
            .AnyAsync(country => country.CountryCode == request.CountryCode && country.IsActive, cancellationToken);

        if (!countryIsActive)
        {
            return false;
        }

        var panelist = await _dbContext.Panelists
            .FirstOrDefaultAsync(p => p.FirebaseId == firebaseId, cancellationToken);

        if (panelist is null)
        {
            return false;
        }

        panelist.Age = request.Age;
        panelist.Gender = request.Gender;
        panelist.CountryCode = request.CountryCode;
        panelist.Onboarded = true;

        if (!panelist.ConsentCollectedOn.HasValue)
        {
            panelist.ConsentCollectedOn = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
