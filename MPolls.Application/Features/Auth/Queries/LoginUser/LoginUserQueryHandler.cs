using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MPolls.Application.Common.Interfaces;
using MPolls.Application.DTOs.Auth;
using MPolls.Domain.Entities;

namespace MPolls.Application.Features.Auth.Queries.LoginUser;

public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, LoginUserResult>
{
    private readonly IFirebaseAuthService _firebaseAuthService;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICountryRepository _countryRepository;
    private static readonly IReadOnlyList<KeyValuePair<int, string>> GenderOptions = new List<KeyValuePair<int, string>>
    {
        new(1, "Male"),
        new(2, "Female"),
        new(3, "Other"),
        new(4, "Prefer not to say"),
    };

    public LoginUserQueryHandler(IFirebaseAuthService firebaseAuthService, IApplicationDbContext dbContext, ICountryRepository countryRepository)
    {
        _firebaseAuthService = firebaseAuthService;
        _dbContext = dbContext;
        _countryRepository = countryRepository;
    }

    public async Task<LoginUserResult> Handle(LoginUserQuery request, CancellationToken cancellationToken)
    {
        // Authenticate with Firebase to obtain the ID token and user profile information.
        var signInResponse = await _firebaseAuthService.SignInAsync(request.Email, request.Password, cancellationToken);

        if (string.IsNullOrWhiteSpace(signInResponse.LocalId))
        {
            throw new InvalidOperationException("Firebase sign-in response did not include a LocalId.");
        }

        var firebaseId = signInResponse.LocalId.Trim();
        var panelistEmail = signInResponse.Email.Trim();

        if (string.IsNullOrWhiteSpace(panelistEmail))
        {
            throw new InvalidOperationException("A valid email address is required to locate the panelist record.");
        }

        // Compare Firebase email verification status with the panelist record.
        var emailVerified = await GetEmailVerifiedStatusAsync(signInResponse.IdToken, cancellationToken);

        var panelist = await FindPanelistAsync(firebaseId, panelistEmail, cancellationToken);

        var hasChanges = panelist is null;

        if (panelist is null)
        {
            panelist = CreatePanelist(firebaseId, panelistEmail, emailVerified);
            await _dbContext.Panelists.AddAsync(panelist, cancellationToken);
        }
        else
        {
            hasChanges = UpdatePanelist(panelist, firebaseId, panelistEmail, emailVerified);
        }

        if (hasChanges)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // Return the login payload with the synchronized verification flag.
        return new LoginUserResult(
            signInResponse.Kind,
            firebaseId,
            panelistEmail,
            signInResponse.DisplayName,
            signInResponse.IdToken,
            signInResponse.Registered,
            signInResponse.RefreshToken,
            signInResponse.ExpiresIn,
            panelist.Ulid,
            panelist.Verified,
            panelist.Onboarded,
            panelist.Age,
            panelist.Gender != null ? GenderOptions[panelist.Gender ?? 1].Value : null,
            _countryRepository.GetByCodeAsync(panelist.CountryCode ?? 0, cancellationToken)?.Result?.CountryName);
    }

    private async Task<Panelist?> FindPanelistAsync(string firebaseId, string panelistEmail, CancellationToken cancellationToken)
    {
        // Attempt a lookup by Firebase ID first, then fall back to the email address.
        var panelist = await _dbContext.Panelists
            .FirstOrDefaultAsync(p => p.FirebaseId == firebaseId, cancellationToken);

        if (panelist is null)
        {
            panelist = await _dbContext.Panelists
                .FirstOrDefaultAsync(p => p.Email == panelistEmail, cancellationToken);
        }

        return panelist;
    }

    private static Panelist CreatePanelist(string firebaseId, string panelistEmail, bool emailVerified)
    {
        // New panelist records are initialized with the Firebase verification state.
        return new Panelist
        {
            FirebaseId = firebaseId,
            Email = panelistEmail,
            Ulid = Ulid.NewUlid().ToString(),
            CreatedDate = DateTime.UtcNow,
            Verified = emailVerified
        };
    }

    private static bool UpdatePanelist(Panelist panelist, string firebaseId, string panelistEmail, bool emailVerified)
    {
        // Synchronize the stored panelist profile with the latest Firebase details.
        var updated = false;

        if (string.IsNullOrWhiteSpace(panelist.Ulid))
        {
            panelist.Ulid = Ulid.NewUlid().ToString();
            updated = true;
        }

        if (!string.Equals(panelist.FirebaseId, firebaseId, StringComparison.Ordinal))
        {
            panelist.FirebaseId = firebaseId;
            updated = true;
        }

        if (!string.Equals(panelist.Email, panelistEmail, StringComparison.OrdinalIgnoreCase))
        {
            panelist.Email = panelistEmail;
            updated = true;
        }

        if (!panelist.Verified && emailVerified)
        {
            panelist.Verified = true;
            updated = true;
        }

        return updated;
    }

    private async Task<bool> GetEmailVerifiedStatusAsync(string idToken, CancellationToken cancellationToken)
    {
        // Missing tokens cannot be verified, so treat them as unverified.
        if (string.IsNullOrWhiteSpace(idToken))
        {
            return false;
        }

        var verificationResponse = await _firebaseAuthService.VerifyIdTokenAsync(idToken, cancellationToken);

        if (!verificationResponse.Claims.TryGetValue("email_verified", out var verifiedValue) || verifiedValue is null)
        {
            return false;
        }

        return verifiedValue switch
        {
            bool booleanValue => booleanValue,
            string stringValue when bool.TryParse(stringValue, out var parsed) => parsed,
            _ => false,
        };
    }
}
