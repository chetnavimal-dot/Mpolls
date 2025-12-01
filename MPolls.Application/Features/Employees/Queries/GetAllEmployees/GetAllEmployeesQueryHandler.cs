using MediatR;
using MPolls.Application.DTOs;
using MPolls.Application.Common.Interfaces;

namespace MPolls.Application.Features.Employees.Queries.GetAllEmployees;

public class GetAllEmployeesQueryHandler 
    : IRequestHandler<GetAllEmployeesQuery, List<EmployeeDto>>
{
    private readonly IEmployeeRepository _employeeRepository;

    public GetAllEmployeesQueryHandler(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<List<EmployeeDto>> Handle(
        GetAllEmployeesQuery request, 
        CancellationToken cancellationToken)
    {
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);

        return employees.Select(e => new EmployeeDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            Salary = e.Salary
        }).ToList();
    }
}
