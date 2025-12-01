using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MPolls.Application.Common.Interfaces;
using MPolls.Application.DTOs.Auth;
using MPolls.Domain.Entities;

namespace MPolls.Application.Features.Auth.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IFirebaseAuthService _firebaseAuthService;
    private readonly IApplicationDbContext _dbContext;

    public RegisterUserCommandHandler(IFirebaseAuthService firebaseAuthService, IApplicationDbContext dbContext)
    {
        _firebaseAuthService = firebaseAuthService;
        _dbContext = dbContext;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var signUpResponse = await _firebaseAuthService.SignUpAsync(request.Email, request.Password, cancellationToken);

        if (string.IsNullOrWhiteSpace(signUpResponse.LocalId))
        {
            throw new InvalidOperationException("Firebase sign-up response did not include a LocalId.");
        }

        await _firebaseAuthService.SendEmailVerificationAsync(signUpResponse.IdToken, cancellationToken);

        var firebaseId = signUpResponse.LocalId.Trim();
        var panelistEmail = request.Email.Trim();

        if (string.IsNullOrWhiteSpace(panelistEmail))
        {
            throw new InvalidOperationException("A valid email address is required to create a panelist record.");
        }

        var responseEmail = signUpResponse.Email?.Trim();

        if (!string.IsNullOrWhiteSpace(responseEmail) &&
            !string.Equals(responseEmail, panelistEmail, StringComparison.OrdinalIgnoreCase))
        {
            panelistEmail = responseEmail;
        }

        var panelist = await _dbContext.Panelists
            .FirstOrDefaultAsync(p => p.Email == panelistEmail, cancellationToken);

        var currentDate = DateTime.Today;
        var hasChanges = false;

        if (panelist is null)
        {
            panelist = new Panelist
            {
                FirebaseId = firebaseId,
                Email = panelistEmail,
                Ulid = Ulid.NewUlid().ToString(),
                CreatedDate = currentDate
            };

            await _dbContext.Panelists.AddAsync(panelist, cancellationToken);
            hasChanges = true;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(panelist.Ulid))
            {
                panelist.Ulid = Ulid.NewUlid().ToString();
                hasChanges = true;
            }

            if (!string.Equals(panelist.FirebaseId, firebaseId, StringComparison.Ordinal))
            {
                panelist.FirebaseId = firebaseId;
                hasChanges = true;
            }

            if (!string.Equals(panelist.Email, panelistEmail, StringComparison.OrdinalIgnoreCase))
            {
                panelist.Email = panelistEmail;
                hasChanges = true;
            }

            if (panelist.CreatedDate != currentDate)
            {
                panelist.CreatedDate = currentDate;
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return new RegisterUserResult(
            signUpResponse.IdToken,
            panelistEmail,
            signUpResponse.RefreshToken,
            signUpResponse.ExpiresIn,
            firebaseId,
            panelist.Ulid);
    }
}
