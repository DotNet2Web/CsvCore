using CsvCore.Specs.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace CsvCore.Specs.Db;

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees { get; set; } = null!;
}
