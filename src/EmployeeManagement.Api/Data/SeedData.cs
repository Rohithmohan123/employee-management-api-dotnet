using EmployeeManagement.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Data;

/// <summary>Small fictional dataset so the API is useful straight after the first migration.</summary>
internal static class SeedData
{
    private static readonly DateTime Seeded = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Apply(ModelBuilder b)
    {
        b.Entity<Department>().HasData(
            new Department { Id = 1, Name = "Engineering", Location = "Dubai" },
            new Department { Id = 2, Name = "Finance", Location = "Dubai" },
            new Department { Id = 3, Name = "Human Resources", Location = "Abu Dhabi" },
            new Department { Id = 4, Name = "Customer Support", Location = "Sharjah" });

        b.Entity<Employee>().HasData(
            E(1, "Aisha", "Al Mansoori", "Head of Engineering", 1, null, 38000, "2018-03-11"),
            E(2, "Rahul", "Nair", "Senior Software Engineer", 1, 1, 24000, "2020-07-01"),
            E(3, "Sara", "Haddad", "Software Engineer", 1, 2, 15500, "2023-02-15"),
            E(4, "Karthik", "Iyer", "QA Engineer", 1, 2, 12500, "2024-05-20"),
            E(5, "Omar", "Khan", "Finance Manager", 2, null, 30000, "2019-09-02"),
            E(6, "Priya", "Sharma", "Financial Analyst", 2, 5, 14000, "2022-11-07"),
            E(7, "Mariam", "Saleh", "HR Manager", 3, null, 28000, "2017-01-16"),
            E(8, "Yusuf", "Rahman", "Support Team Lead", 4, null, 16000, "2021-04-12"),
            E(9, "Neha", "Menon", "Support Agent", 4, 8, 7500, "2025-01-06"));

        b.Entity<Project>().HasData(
            new Project { Id = 1, Code = "ENG-PORTAL", Name = "Customer self-service portal", DepartmentId = 1, StartDate = new DateOnly(2025, 2, 1) },
            new Project { Id = 2, Code = "FIN-CLOSE", Name = "Faster month-end close", DepartmentId = 2, StartDate = new DateOnly(2025, 1, 10), EndDate = new DateOnly(2025, 9, 30) },
            new Project { Id = 3, Code = "SUP-KB", Name = "Support knowledge base", DepartmentId = 4, StartDate = new DateOnly(2025, 3, 1) });

        b.Entity<ProjectAssignment>().HasData(
            new ProjectAssignment { EmployeeId = 2, ProjectId = 1, Role = "Tech lead", AllocationPercent = 60 },
            new ProjectAssignment { EmployeeId = 3, ProjectId = 1, Role = "Developer", AllocationPercent = 100 },
            new ProjectAssignment { EmployeeId = 4, ProjectId = 1, Role = "Tester", AllocationPercent = 50 },
            new ProjectAssignment { EmployeeId = 6, ProjectId = 2, Role = "Analyst", AllocationPercent = 40 },
            new ProjectAssignment { EmployeeId = 9, ProjectId = 3, Role = "Content writer", AllocationPercent = 25 });
    }

    private static Employee E(int id, string first, string last, string title, int dept, int? manager, decimal salary, string hired) => new()
    {
        Id = id, FirstName = first, LastName = last, JobTitle = title, DepartmentId = dept, ManagerId = manager,
        Email = $"{first}.{last.Replace("Al ", "al").Replace(" ", "")}@example.com".ToLowerInvariant(),
        MonthlySalary = salary, HireDate = DateOnly.ParseExact(hired, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), CreatedAtUtc = Seeded, UpdatedAtUtc = Seeded
    };
}
