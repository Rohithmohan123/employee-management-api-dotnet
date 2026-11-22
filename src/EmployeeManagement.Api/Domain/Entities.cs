namespace EmployeeManagement.Api.Domain;

public enum EmployeeStatus { Active, OnLeave, Terminated }

public class Department
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Location { get; set; }
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}

public class Employee
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required string JobTitle { get; set; }
    public DateOnly HireDate { get; set; }
    public decimal MonthlySalary { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    /// <summary>Optional reporting line (self-referencing relationship).</summary>
    public int? ManagerId { get; set; }
    public Employee? Manager { get; set; }
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

    public ICollection<ProjectAssignment> Assignments { get; set; } = new List<ProjectAssignment>();
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class Project
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public ICollection<ProjectAssignment> Assignments { get; set; } = new List<ProjectAssignment>();
}

/// <summary>Join entity for the many-to-many relationship between employees and projects.</summary>
public class ProjectAssignment
{
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    public required string Role { get; set; }
    public int AllocationPercent { get; set; }
}
