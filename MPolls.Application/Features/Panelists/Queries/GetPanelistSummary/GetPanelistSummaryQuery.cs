using System;
using MediatR;

namespace MPolls.Application.Features.Panelists.Queries.GetPanelistSummary;

public sealed record GetPanelistSummaryQuery(string FirebaseId) : IRequest<PanelistSummary?>;

public sealed record PanelistSummary(
    Guid PanelistId,
    string FirebaseId,
    string Email,
    string Ulid,
    bool Verified,
    bool Onboarded,
    int? Age,
    string? Gender,
    string? Country);
