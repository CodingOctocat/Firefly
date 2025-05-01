using Firefly.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Firefly.Migrations;

public class CccfDesignTimeDbContextFactory : IDesignTimeDbContextFactory<CccfDbContext>
{
    public CccfDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CccfDbContext>();
        optionsBuilder.UseSqlite("Data Source=cccf.db");

        return new CccfDbContext(optionsBuilder.Options);
    }
}
