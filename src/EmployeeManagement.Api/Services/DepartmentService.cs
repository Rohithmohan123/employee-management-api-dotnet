using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Domain;
using EmployeeManagement.Api.Dtos;
using EmployeeManagement.Api.Errors;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services;

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentDto>> ListAsync(CancellationToken ct);
    Task<DepartmentDto> GetAsync(int id, CancellationToken ct);
    Task<DepartmentDto> CreateAsync(DepartmentRequest request, CancellationToken ct);
    Task<DepartmentDto> UpdateAsync(int id, DepartmentRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<ProjectDto>> ListProjectsAsync(CancellationToken ct);
}

public sealed class DepartmentService(AppDbContext db) : IDepartmentService
{
    public async Task<IReadOnlyList<DepartmentDto>> ListAsync(CancellationToken ct) =>
        await db.Departments.AsNoTracking().OrderBy(d => d.Name)
            .Select(d => new DepartmentDto(d.Id, d.Name, d.Location, d.Employees.Count(e => e.Status != EmployeeStatus.Terminated)))
            .ToListAsync(ct);

    public async Task<DepartmentDto> GetAsync(int id, CancellationToken ct) =>
        await db.Departments.AsNoTracking().Where(d => d.Id == id)
            .Select(d => new DepartmentDto(d.Id, d.Name, d.Location, d.Employees.Count(e => e.Status != EmployeeStatus.Terminated)))
            .FirstOrDefaultAsync(ct) ?? throw new NotFoundException("Department", id);

    public async Task<DepartmentDto> CreateAsync(DepartmentRequest r, CancellationToken ct)
    {
        await EnsureUniqueNameAsync(r.Name, null, ct);
        var d = new Department { Name = r.Name.Trim(), Location = r.Location.Trim() };
        db.Departments.Add(d);
        await db.SaveChangesAsync(ct);
        return await GetAsync(d.Id, ct);
    }

    public async Task<DepartmentDto> UpdateAsync(int id, DepartmentRequest r, CancellationToken ct)
    {
        var d = await db.Departments.FindAsync([id], ct) ?? throw new NotFoundException("Department", id);
        await EnsureUniqueNameAsync(r.Name, id, ct);
        d.Name = r.Name.Trim();
        d.Location = r.Location.Trim();
        await db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var d = await db.Departments.FindAsync([id], ct) ?? throw new NotFoundException("Department", id);
        if (await db.Employees.AnyAsync(e => e.DepartmentId == id, ct) || await db.Projects.AnyAsync(p => p.DepartmentId == id, ct))
            throw new ConflictException("The department still has employees or projects. Move them before deleting it.");
        db.Departments.Remove(d);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ProjectDto>> ListProjectsAsync(CancellationToken ct) =>
        await db.Projects.AsNoTracking().OrderBy(p => p.Code)
            .Select(p => new ProjectDto(p.Id, p.Code, p.Name, p.DepartmentId, p.Department!.Name, p.StartDate, p.EndDate, p.Assignments.Count))
            .ToListAsync(ct);

    private async Task EnsureUniqueNameAsync(string name, int? id, CancellationToken ct)
    {
        var n = name.Trim();
        if (await db.Departments.AnyAsync(d => d.Name == n && d.Id != id, ct))
            throw new ConflictException($"A department named '{n}' already exists.");
    }
}
