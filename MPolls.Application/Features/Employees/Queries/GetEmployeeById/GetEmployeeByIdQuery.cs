using MediatR;
using MPolls.Application.DTOs;

namespace MPolls.Application.Features.Employees.Queries.GetEmployeeById;

public record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeDto?>;
