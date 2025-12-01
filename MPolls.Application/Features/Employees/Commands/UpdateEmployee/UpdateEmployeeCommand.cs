using MediatR;
using MPolls.Application.DTOs;

namespace MPolls.Application.Features.Employees.Commands.UpdateEmployee;

public record UpdateEmployeeCommand(Guid Id, string FirstName, string LastName, string Email, decimal Salary) : IRequest<EmployeeDto?>;
