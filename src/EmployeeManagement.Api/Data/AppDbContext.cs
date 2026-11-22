using EmployeeManagement.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectAssignment> ProjectAssignments => Set<ProjectAssignment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Department>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.Location).HasMaxLength(100);
            e.HasIndex(x => x.Name).IsUnique();
        });

        b.Entity<Employee>(e =>
        {
            e.Property(x => x.FirstName).HasMaxLength(50);
            e.Property(x => x.LastName).HasMaxLength(50);
            e.Property(x => x.Email).HasMaxLength(254);
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.JobTitle).HasMaxLength(100);
            e.Property(x => x.MonthlySalary).HasPrecision(10, 2);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(12);
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => new { x.LastName, x.FirstName });

            // Deleting a department that still has staff is blocked (Restrict); the service returns 409.
            e.HasOne(x => x.Department).WithMany(d => d.Employees).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Manager).WithMany(m => m.DirectReports).HasForeignKey(x => x.ManagerId).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Employees_Salary", "[MonthlySalary] > 0");
                t.HasCheckConstraint("CK_Employees_NotOwnManager", "[ManagerId] IS NULL OR [ManagerId] <> [Id]");
            });
        });

        b.Entity<Project>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(12);
            e.Property(x => x.Name).HasMaxLength(120);
            e.HasIndex(x => x.Code).IsUnique();
            e.HasOne(x => x.Department).WithMany(d => d.Projects).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ProjectAssignment>(e =>
        {
            e.HasKey(x => new { x.EmployeeId, x.ProjectId });
            e.Property(x => x.Role).HasMaxLength(60);
            e.HasOne(x => x.Employee).WithMany(x => x.Assignments).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Project).WithMany(x => x.Assignments).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            e.ToTable(t => t.HasCheckConstraint("CK_ProjectAssignments_Allocation", "[AllocationPercent] BETWEEN 1 AND 100"));
        });

        SeedData.Apply(b);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Employee>())
        {
            if (entry.State == EntityState.Added) entry.Entity.CreatedAtUtc = now;
            if (entry.State is EntityState.Added or EntityState.Modified) entry.Entity.UpdatedAtUtc = now;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
