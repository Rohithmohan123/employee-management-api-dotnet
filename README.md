# Employee Management API – ASP.NET Core, EF Core, SQL Server

> **Independent demonstration project.** I built this on my own to practise and show C#, ASP.NET Core and SQL Server skills. It is **not** work from a previous employer. All data is fictional.

A REST API for managing departments, employees and project assignments, built with **ASP.NET Core 8 Web API**, **Entity Framework Core 8** and **SQL Server**. It shows layered design, relational modelling, input validation, consistent error handling, automated tests and a Postman collection that CI runs against a real SQL Server.

## Features

- CRUD for **departments** and **employees**; read-only **projects**; assign and unassign employees to projects
- Relationships: department → employees (one-to-many), employee → manager (self-referencing), employees ↔ projects (many-to-many with role and allocation)
- Search, filtering (department, status), sorting and paging on the employee list
- Validation with data annotations plus business rules in the service layer:
  - unique email (409), department and manager must exist (422), an employee cannot manage themselves
  - total project allocation ≤ 100% (422), a manager with direct reports cannot be deleted (409)
  - a department with staff or projects cannot be deleted (409)
- Global exception handling that returns **RFC 7807 problem details** (`application/problem+json`) and never leaks stack traces
- EF Core **migrations**, seed data, unique indexes and check constraints
- Swagger UI in development, `/health` endpoint with a database check

## Technology

| Area | Choice |
|---|---|
| Language / runtime | C# 12, .NET 8 |
| Web | ASP.NET Core Web API (controllers), Swagger (Swashbuckle) |
| Data | Entity Framework Core 8, SQL Server 2019+ |
| Tests | xUnit, `WebApplicationFactory`, SQLite in-memory; Postman collection run by Newman |
| CI | GitHub Actions: build, tests, then the API against SQL Server 2022 with the Postman collection |

## Architecture

```
HTTP ─▶ Controllers ─▶ Services (business rules) ─▶ AppDbContext (EF Core) ─▶ SQL Server
          │                 │
          │                 └─ throws NotFound / Conflict / BusinessRule exceptions
          └─ DTOs + data-annotation validation (400 ValidationProblemDetails)
GlobalExceptionHandler ─▶ maps exceptions to 404 / 409 / 422 / 500 problem details
```

## Project structure

```
├── src/EmployeeManagement.Api/
│   ├── Controllers/          # EmployeesController, DepartmentsController, ProjectsController
│   ├── Services/             # EmployeeService, DepartmentService (business rules)
│   ├── Data/                 # AppDbContext (Fluent API configuration), SeedData
│   ├── Domain/Entities.cs    # Department, Employee, Project, ProjectAssignment
│   ├── Dtos/Dtos.cs          # request/response records with validation attributes
│   ├── Errors/               # custom exceptions + GlobalExceptionHandler
│   ├── Migrations/           # EF Core migrations
│   └── Program.cs
├── tests/EmployeeManagement.Tests/   # xUnit integration tests over HTTP
├── postman/EmployeeManagement.postman_collection.json
├── docs/schema.sql           # idempotent SQL script generated from the migrations
└── .github/workflows/ci.yml
```

## Endpoints

| Method | Route | Description | Success | Errors |
|---|---|---|---|---|
| GET | `/api/employees?search=&departmentId=&status=&sort=&page=&pageSize=` | Paged list | 200 | 400 |
| GET | `/api/employees/{id}` | Employee with department, manager and projects | 200 | 404 |
| POST | `/api/employees` | Create | 201 + `Location` | 400, 409, 422 |
| PUT | `/api/employees/{id}` | Update | 200 | 400, 404, 409, 422 |
| DELETE | `/api/employees/{id}` | Delete | 204 | 404, 409 |
| POST | `/api/employees/{id}/projects` | Assign to project | 200 | 404, 409, 422 |
| DELETE | `/api/employees/{id}/projects/{projectId}` | Remove assignment | 204 | 404 |
| GET / POST | `/api/departments` | List / create | 200 / 201 | 409 |
| GET / PUT / DELETE | `/api/departments/{id}` | Get / update / delete | 200 / 200 / 204 | 404, 409 |
| GET | `/api/projects` | List projects | 200 | – |
| GET | `/health` | Health check (includes database) | 200 | 503 |

`sort` accepts `name` (default), `hireDate` or `salary`, prefixed with `-` for descending.

Example error:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "An employee with the email 'rahul.nair@example.com' already exists.",
  "instance": "/api/employees"
}
```

## Setup

Requirements: [.NET 8 SDK](https://dotnet.microsoft.com/download) and SQL Server 2019+ (LocalDB, Express, Developer, or Docker).

1. Start SQL Server (Docker example):

   ```bash
   docker run -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD='Your_Strong_Passw0rd' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
   ```

2. Set the connection string. Don't commit real passwords; use user secrets or an environment variable:

   ```bash
   cd src/EmployeeManagement.Api
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost,1433;Database=EmployeeManagement;User Id=sa;Password=Your_Strong_Passw0rd;TrustServerCertificate=True"
   ```

3. Run the API. In Development, migrations are applied automatically on start-up:

   ```bash
   dotnet run --project src/EmployeeManagement.Api
   ```

   Open http://localhost:5080/swagger. To apply migrations manually instead: `dotnet tool install -g dotnet-ef` then `dotnet ef database update --project src/EmployeeManagement.Api`.

## Testing

```bash
dotnet test
```

The integration tests start the real API in memory with `WebApplicationFactory` and an in-memory SQLite database. They cover listing and paging, filters, 404 problem details, the full create → update → delete cycle, validation errors (400), duplicate email (409), unknown department (422), delete rules (409), assignment rules and the health check.

**Postman:** import `postman/EmployeeManagement.postman_collection.json`, set `baseUrl` (default `http://localhost:5080`) and run the collection. It has 18 requests with tests. In CI it runs with Newman against the API on SQL Server 2022, after EF Core migrations have been applied.

## Limitations

- No authentication or authorisation; adding JWT bearer auth and role policies would be the next step.
- Salaries are stored without currency or history (see the companion SQL Server project for salary history).
- The SQLite test database cannot sort by `decimal`, so salary sorting is covered by the Postman run on SQL Server rather than the xUnit tests.

## Licence

MIT, see [LICENSE](LICENSE).
