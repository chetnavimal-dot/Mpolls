using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPolls.Application.DTOs;
using MPolls.Application.Features.Employees.Commands.AddEmployee;
using MPolls.Application.Features.Employees.Commands.UpdateEmployee;
using MPolls.Application.Features.Employees.Commands.DeleteEmployee;
using MPolls.Application.Features.Employees.Queries.GetAllEmployees;
using MPolls.Application.Features.Employees.Queries.GetEmployeeById;

namespace MPolls.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Add([FromBody] AddEmployeeCommand command)
    {
        var employee = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var employees = await _mediator.Send(new GetAllEmployeesQuery());
        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var employee = await _mediator.Send(new GetEmployeeByIdQuery(id));
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        var employee = await _mediator.Send(command);
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _mediator.Send(new DeleteEmployeeCommand(id));
        return success ? NoContent() : NotFound();
    }
}
