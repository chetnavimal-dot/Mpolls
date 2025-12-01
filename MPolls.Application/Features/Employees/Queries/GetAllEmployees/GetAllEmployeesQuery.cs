using MediatR;
using MPolls.Application.DTOs;

namespace MPolls.Application.Features.Employees.Queries.GetAllEmployees;

public record GetAllEmployeesQuery : IRequest<List<EmployeeDto>>;
