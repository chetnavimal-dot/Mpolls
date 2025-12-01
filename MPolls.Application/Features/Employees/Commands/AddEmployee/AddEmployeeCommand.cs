using MediatR;
using MPolls.Application.DTOs;

namespace MPolls.Application.Features.Employees.Commands.AddEmployee;

public record AddEmployeeCommand(string FirstName, string LastName, string Email, decimal Salary) : IRequest<EmployeeDto>;
