using Microsoft.EntityFrameworkCore;
using sdrproj.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Merchant> Merchants { get; set; }
}

