using MediatR;
using MPolls.Application.DTOs;

namespace MPolls.Application.Features.Dashboard.Queries;

public record GetDashboardDetailsQuery(string PanelistUlid) : IRequest<DashboardResponse>;