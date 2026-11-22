using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Domain;
using EmployeeManagement.Api.Dtos;
using EmployeeManagement.Api.Errors;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services;

public interface IEmployeeService
{
    Task<PagedResult<EmployeeSummaryDto>> ListAsync(EmployeeQuery query, CancellationToken ct);
    Task<EmployeeDto> GetAsync(int id, CancellationToken ct);
    Task<EmployeeDto> CreateAsync(EmployeeRequest request, CancellationToken ct);
    Task<EmployeeDto> UpdateAsync(int id, EmployeeRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<EmployeeDto> AssignAsync(int id, AssignmentRequest request, CancellationToken ct);
    Task UnassignAsync(int id, int projectId, CancellationToken ct);
}

public sealed class EmployeeService(AppDbContext db) : IEmployeeService
{
    public async Task<PagedResult<EmployeeSummaryDto>> ListAsync(EmployeeQuery q, CancellationToken ct)
    {
        var query = db.Employees.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            // LIKE is case-insensitive on SQL Server's default collation (and on SQLite for ASCII); escape wildcards
            var pattern = "%" + q.Search.Trim().Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";
            query = query.Where(e => EF.Functions.Like(e.FirstName, pattern) || EF.Functions.Like(e.LastName, pattern)
                                  || EF.Functions.Like(e.Email, pattern) || EF.Functions.Like(e.JobTitle, pattern));
        }
        if (q.DepartmentId is int dept) query = query.Where(e => e.DepartmentId == dept);
        if (q.Status is EmployeeStatus status) query = query.Where(e => e.Status == status);

        query = q.Sort switch
        {
            "hireDate" => query.OrderBy(e => e.HireDate).ThenBy(e => e.Id),
            "-hireDate" => query.OrderByDescending(e => e.HireDate).ThenBy(e => e.Id),
            "salary" => query.OrderBy(e => e.MonthlySalary).ThenBy(e => e.Id),
            "-salary" => query.OrderByDescending(e => e.MonthlySalary).ThenBy(e => e.Id),
            "-name" => query.OrderByDescending(e => e.LastName).ThenByDescending(e => e.FirstName).ThenBy(e => e.Id),
            _ => query.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ThenBy(e => e.Id)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(e => new EmployeeSummaryDto(e.Id, e.FirstName + " " + e.LastName, e.Email, e.JobTitle, e.Department!.Name, e.Status))
            .ToListAsync(ct);
        return new PagedResult<EmployeeSummaryDto>(items, q.Page, q.PageSize, total);
    }

    public async Task<EmployeeDto> GetAsync(int id, CancellationToken ct) =>
        await db.Employees.AsNoTracking().Where(e => e.Id == id).Select(ToDto).FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException("Employee", id);

    public async Task<EmployeeDto> CreateAsync(EmployeeRequest r, CancellationToken ct)
    {
        await ValidateAsync(r, null, ct);
        var e = new Employee
        {
            FirstName = r.FirstName.Trim(), LastName = r.LastName.Trim(), Email = r.Email.Trim().ToLowerInvariant(), Phone = r.Phone,
            JobTitle = r.JobTitle.Trim(), HireDate = r.HireDate!.Value, MonthlySalary = r.MonthlySalary,
            DepartmentId = r.DepartmentId, ManagerId = r.ManagerId, Status = r.Status
        };
        db.Employees.Add(e);
        await db.SaveChangesAsync(ct);
        return await GetAsync(e.Id, ct);
    }

    public async Task<EmployeeDto> UpdateAsync(int id, EmployeeRequest r, CancellationToken ct)
    {
        var e = await db.Employees.FindAsync([id], ct) ?? throw new NotFoundException("Employee", id);
        await ValidateAsync(r, id, ct);
        e.FirstName = r.FirstName.Trim(); e.LastName = r.LastName.Trim(); e.Email = r.Email.Trim().ToLowerInvariant();
        e.Phone = r.Phone; e.JobTitle = r.JobTitle.Trim(); e.HireDate = r.HireDate!.Value; e.MonthlySalary = r.MonthlySalary;
        e.DepartmentId = r.DepartmentId; e.ManagerId = r.ManagerId; e.Status = r.Status;
        await db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var e = await db.Employees.FindAsync([id], ct) ?? throw new NotFoundException("Employee", id);
        if (await db.Employees.AnyAsync(x => x.ManagerId == id, ct))
            throw new ConflictException("This employee manages other employees. Reassign their direct reports first.");
        db.Employees.Remove(e);                 // project assignments are removed by cascade
        await db.SaveChangesAsync(ct);
    }

    public async Task<EmployeeDto> AssignAsync(int id, AssignmentRequest r, CancellationToken ct)
    {
        if (!await db.Employees.AnyAsync(e => e.Id == id, ct)) throw new NotFoundException("Employee", id);
        if (!await db.Projects.AnyAsync(p => p.Id == r.ProjectId, ct)) throw new BusinessRuleException($"Project {r.ProjectId} does not exist.");
        if (await db.ProjectAssignments.AnyAsync(a => a.EmployeeId == id && a.ProjectId == r.ProjectId, ct))
            throw new ConflictException("The employee is already assigned to this project.");
        var allocated = await db.ProjectAssignments.Where(a => a.EmployeeId == id).SumAsync(a => (int?)a.AllocationPercent, ct) ?? 0;
        if (allocated + r.AllocationPercent > 100)
            throw new BusinessRuleException($"Total allocation would be {allocated + r.AllocationPercent}%. The maximum is 100%.");

        db.ProjectAssignments.Add(new ProjectAssignment { EmployeeId = id, ProjectId = r.ProjectId, Role = r.Role.Trim(), AllocationPercent = r.AllocationPercent });
        await db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task UnassignAsync(int id, int projectId, CancellationToken ct)
    {
        var a = await db.ProjectAssignments.FindAsync([id, projectId], ct) ?? throw new NotFoundException("Assignment", $"{id}/{projectId}");
        db.ProjectAssignments.Remove(a);
        await db.SaveChangesAsync(ct);
    }

    private async Task ValidateAsync(EmployeeRequest r, int? id, CancellationToken ct)
    {
        var email = r.Email.Trim().ToLowerInvariant();
        if (await db.Employees.AnyAsync(e => e.Email == email && e.Id != id, ct))
            throw new ConflictException($"An employee with the email '{email}' already exists.");
        if (!await db.Departments.AnyAsync(d => d.Id == r.DepartmentId, ct))
            throw new BusinessRuleException($"Department {r.DepartmentId} does not exist.");
        if (r.ManagerId is int managerId)
        {
            if (managerId == id) throw new BusinessRuleException("An employee cannot be their own manager.");
            var manager = await db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == managerId, ct)
                ?? throw new BusinessRuleException($"Manager {managerId} does not exist.");
            if (manager.Status == EmployeeStatus.Terminated) throw new BusinessRuleException("The selected manager is no longer employed.");
        }
    }

    private static readonly System.Linq.Expressions.Expression<Func<Employee, EmployeeDto>> ToDto = e => new EmployeeDto(
        e.Id, e.FirstName, e.LastName, e.Email, e.Phone, e.JobTitle, e.HireDate, e.MonthlySalary, e.Status,
        e.DepartmentId, e.Department!.Name, e.ManagerId,
        e.Manager == null ? null : e.Manager.FirstName + " " + e.Manager.LastName,
        e.Assignments.OrderBy(a => a.Project!.Code)
            .Select(a => new AssignmentDto(a.ProjectId, a.Project!.Code, a.Project.Name, a.Role, a.AllocationPercent)).ToList());
}
