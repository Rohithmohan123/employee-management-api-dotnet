using EmployeeManagement.Api.Dtos;
using EmployeeManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

/// <summary>CRUD for departments.</summary>
[ApiController]
[Route("api/departments")]
[Produces("application/json")]
public class DepartmentsController(IDepartmentService departments) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<DepartmentDto>> List(CancellationToken ct) => departments.ListAsync(ct);

    [HttpGet("{id:int}")]
    [ProducesResponseType<DepartmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<DepartmentDto> Get(int id, CancellationToken ct) => departments.GetAsync(id, ct);

    [HttpPost]
    [ProducesResponseType<DepartmentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DepartmentDto>> Create(DepartmentRequest request, CancellationToken ct)
    {
        var created = await departments.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<DepartmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<DepartmentDto> Update(int id, DepartmentRequest request, CancellationToken ct) => departments.UpdateAsync(id, request, ct);

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await departments.DeleteAsync(id, ct);
        return NoContent();
    }
}

/// <summary>Read-only list of projects (used when assigning employees).</summary>
[ApiController]
[Route("api/projects")]
[Produces("application/json")]
public class ProjectsController(IDepartmentService departments) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ProjectDto>> List(CancellationToken ct) => departments.ListProjectsAsync(ct);
}
