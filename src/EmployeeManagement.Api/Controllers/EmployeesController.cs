using EmployeeManagement.Api.Dtos;
using EmployeeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

/// <summary>CRUD for employees, plus their project assignments.</summary>
[ApiController]
[Route("api/employees")]
[Produces("application/json")]
public class EmployeesController(IEmployeeService employees) : ControllerBase
{
    /// <summary>List employees with optional search, filters, sorting and paging.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<EmployeeSummaryDto>>(StatusCodes.Status200OK)]
    public Task<PagedResult<EmployeeSummaryDto>> List([FromQuery] EmployeeQuery query, CancellationToken ct) => employees.ListAsync(query, ct);

    /// <summary>Get one employee with department, manager and projects.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<EmployeeDto> Get(int id, CancellationToken ct) => employees.GetAsync(id, ct);

    /// <summary>Create an employee.</summary>
    [HttpPost]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<EmployeeDto>> Create(EmployeeRequest request, CancellationToken ct)
    {
        var created = await employees.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Replace an employee's details.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<EmployeeDto> Update(int id, EmployeeRequest request, CancellationToken ct) => employees.UpdateAsync(id, request, ct);

    /// <summary>Delete an employee (blocked while they manage others).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await employees.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>Assign the employee to a project (total allocation capped at 100%).</summary>
    [HttpPost("{id:int}/projects")]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public Task<EmployeeDto> Assign(int id, AssignmentRequest request, CancellationToken ct) => employees.AssignAsync(id, request, ct);

    /// <summary>Remove a project assignment.</summary>
    [HttpDelete("{id:int}/projects/{projectId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unassign(int id, int projectId, CancellationToken ct)
    {
        await employees.UnassignAsync(id, projectId, ct);
        return NoContent();
    }
}
