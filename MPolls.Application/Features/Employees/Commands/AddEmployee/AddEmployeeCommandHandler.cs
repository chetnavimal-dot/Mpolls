using MediatR;
using MPolls.Application.Common.Interfaces;
using MPolls.Application.DTOs;
using MPolls.Domain.Entities;

namespace MPolls.Application.Features.Employees.Commands.AddEmployee;

public class AddEmployeeCommandHandler : IRequestHandler<AddEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _employeeRepository;

    public AddEmployeeCommandHandler(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<EmployeeDto> Handle(AddEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Salary = request.Salary
        };

        await _employeeRepository.AddAsync(employee, cancellationToken);

        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Salary = employee.Salary
        };
    }
}
