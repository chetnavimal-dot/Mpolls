using MediatR;

namespace MPolls.Application.Features.Employees.Commands.DeleteEmployee;

public record DeleteEmployeeCommand(Guid Id) : IRequest<bool>;
