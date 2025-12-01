using MediatR;

namespace MPolls.Application.Features.Panelists.Queries.GetPanelistUlid;

public sealed record GetPanelistUlidQuery(string FirebaseId) : IRequest<string?>;
