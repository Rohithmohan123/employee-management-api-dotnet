using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EmployeeManagement.Api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Tests;

public class EmployeesApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    private readonly HttpClient _client = factory.CreateClient();

    private static object NewEmployee(string email, int departmentId = 1, int? managerId = 1, decimal salary = 9000) => new
    {
        firstName = "Test", lastName = "Engineer", email, jobTitle = "Junior Software Engineer",
        hireDate = "2026-02-01", monthlySalary = salary, departmentId, managerId
    };

    [Fact]
    public async Task List_returns_seeded_employees_with_paging_metadata()
    {
        var page = await _client.GetFromJsonAsync<PagedResult<EmployeeSummaryDto>>("/api/employees?pageSize=5", Json);
        Assert.NotNull(page);
        Assert.Equal(5, page.Items.Count);
        Assert.True(page.TotalCount >= 9);
        Assert.Equal((int)Math.Ceiling(page.TotalCount / 5.0), page.TotalPages);
    }

    [Fact]
    public async Task List_filters_by_department_and_search()
    {
        var page = await _client.GetFromJsonAsync<PagedResult<EmployeeSummaryDto>>("/api/employees?departmentId=2", Json);
        Assert.All(page!.Items, e => Assert.Equal("Finance", e.Department));
        var search = await _client.GetFromJsonAsync<PagedResult<EmployeeSummaryDto>>("/api/employees?search=haddad", Json);
        Assert.Single(search!.Items);
    }

    [Fact]
    public async Task Get_returns_employee_with_manager_and_projects()
    {
        var e = await _client.GetFromJsonAsync<EmployeeDto>("/api/employees/3", Json);
        Assert.Equal("Sara", e!.FirstName);
        Assert.Equal("Rahul Nair", e.ManagerName);
        Assert.Contains(e.Projects, p => p.ProjectCode == "ENG-PORTAL");
    }

    [Fact]
    public async Task Get_unknown_id_returns_404_problem_details()
    {
        var res = await _client.GetAsync("/api/employees/9999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        var problem = await res.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Resource not found", problem!.Title);
    }

    [Fact]
    public async Task Create_then_update_then_delete()
    {
        var create = await _client.PostAsJsonAsync("/api/employees", NewEmployee("crud.test@example.com"));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<EmployeeDto>(Json);
        Assert.NotNull(create.Headers.Location);

        var update = await _client.PutAsJsonAsync($"/api/employees/{created!.Id}", NewEmployee("crud.test@example.com", salary: 9500));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(9500, (await update.Content.ReadFromJsonAsync<EmployeeDto>(Json))!.MonthlySalary);

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/employees/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/employees/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Create_with_invalid_body_returns_400_with_field_errors()
    {
        var res = await _client.PostAsJsonAsync("/api/employees", new { firstName = "", email = "not-an-email", monthlySalary = 5, departmentId = 0 });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var problem = await res.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("Email", problem!.Errors.Keys);
        Assert.Contains("FirstName", problem.Errors.Keys);
        Assert.Contains("MonthlySalary", problem.Errors.Keys);
    }

    [Fact]
    public async Task Duplicate_email_returns_409()
    {
        var res = await _client.PostAsJsonAsync("/api/employees", NewEmployee("rahul.nair@example.com"));
        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
    }

    [Fact]
    public async Task Unknown_department_returns_422()
    {
        var res = await _client.PostAsJsonAsync("/api/employees", NewEmployee("dept.test@example.com", departmentId: 99));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, res.StatusCode);
    }

    [Fact]
    public async Task Deleting_a_manager_with_reports_returns_409()
    {
        var res = await _client.DeleteAsync("/api/employees/1");
        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
    }

    [Fact]
    public async Task Assignment_rules_duplicate_and_allocation_cap()
    {
        // Employee 3 is already 100% on ENG-PORTAL
        var dup = await _client.PostAsJsonAsync("/api/employees/3/projects", new { projectId = 1, role = "Developer", allocationPercent = 10 });
        Assert.Equal(HttpStatusCode.Conflict, dup.StatusCode);
        var over = await _client.PostAsJsonAsync("/api/employees/3/projects", new { projectId = 3, role = "Reviewer", allocationPercent = 10 });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, over.StatusCode);

        var ok = await _client.PostAsJsonAsync("/api/employees/4/projects", new { projectId = 3, role = "Reviewer", allocationPercent = 20 });
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync("/api/employees/4/projects/3")).StatusCode);
    }
}

public class DepartmentsApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Department_crud_and_delete_rules()
    {
        var create = await _client.PostAsJsonAsync("/api/departments", new { name = "Legal", location = "Dubai" });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var dept = await create.Content.ReadFromJsonAsync<DepartmentDto>();

        Assert.Equal(HttpStatusCode.Conflict, (await _client.PostAsJsonAsync("/api/departments", new { name = "Legal", location = "Abu Dhabi" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.PutAsJsonAsync($"/api/departments/{dept!.Id}", new { name = "Legal & Compliance", location = "Dubai" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _client.DeleteAsync("/api/departments/1")).StatusCode);   // has employees
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/departments/{dept.Id}")).StatusCode);
    }

    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        var res = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}
