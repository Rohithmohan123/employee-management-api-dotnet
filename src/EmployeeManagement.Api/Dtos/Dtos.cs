using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Api.Domain;

namespace EmployeeManagement.Api.Dtos;

// ---------- Departments ----------
public record DepartmentDto(int Id, string Name, string Location, int EmployeeCount);

public record DepartmentRequest(
    [Required, StringLength(100, MinimumLength = 2)] string Name,
    [Required, StringLength(100, MinimumLength = 2)] string Location);

// ---------- Employees ----------
public record EmployeeSummaryDto(int Id, string FullName, string Email, string JobTitle, string Department, EmployeeStatus Status);

public record EmployeeDto(
    int Id, string FirstName, string LastName, string Email, string? Phone, string JobTitle,
    DateOnly HireDate, decimal MonthlySalary, EmployeeStatus Status,
    int DepartmentId, string Department, int? ManagerId, string? ManagerName,
    IReadOnlyList<AssignmentDto> Projects);

public record EmployeeRequest : IValidatableObject
{
    [Required, StringLength(50, MinimumLength = 1)] public string FirstName { get; init; } = "";
    [Required, StringLength(50, MinimumLength = 1)] public string LastName { get; init; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; init; } = "";
    [Phone, StringLength(20)] public string? Phone { get; init; }
    [Required, StringLength(100, MinimumLength = 2)] public string JobTitle { get; init; } = "";
    [Required] public DateOnly? HireDate { get; init; }
    [Range(typeof(decimal), "1000", "500000", ErrorMessage = "Monthly salary must be between 1,000 and 500,000.")] public decimal MonthlySalary { get; init; }
    [Range(1, int.MaxValue, ErrorMessage = "Choose a department.")] public int DepartmentId { get; init; }
    public int? ManagerId { get; init; }
    public EmployeeStatus Status { get; init; } = EmployeeStatus.Active;

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (HireDate is { } d && d > DateOnly.FromDateTime(DateTime.UtcNow.AddDays(90)))
            yield return new ValidationResult("Hire date cannot be more than 90 days in the future.", [nameof(HireDate)]);
        if (!Enum.IsDefined(Status))
            yield return new ValidationResult("Status must be Active, OnLeave or Terminated.", [nameof(Status)]);
    }
}

public record EmployeeQuery
{
    public string? Search { get; init; }
    public int? DepartmentId { get; init; }
    public EmployeeStatus? Status { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 100)] public int PageSize { get; init; } = 20;
    /// <summary>name (default), hireDate or salary; prefix with "-" for descending.</summary>
    public string? Sort { get; init; }
}

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

// ---------- Projects & assignments ----------
public record ProjectDto(int Id, string Code, string Name, int DepartmentId, string Department, DateOnly StartDate, DateOnly? EndDate, int TeamSize);

public record AssignmentDto(int ProjectId, string ProjectCode, string ProjectName, string Role, int AllocationPercent);

public record AssignmentRequest(
    [Range(1, int.MaxValue)] int ProjectId,
    [Required, StringLength(60, MinimumLength = 2)] string Role,
    [Range(1, 100)] int AllocationPercent);
