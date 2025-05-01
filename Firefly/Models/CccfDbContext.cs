using Firefly.Models.Responses;

using Microsoft.EntityFrameworkCore;

namespace Firefly.Models;

public sealed class CccfDbContext : DbContext
{
    public DbSet<Cccf> Products { get; set; }

    public CccfDbContext(DbContextOptions<CccfDbContext> options) : base(options)
    { }
}
