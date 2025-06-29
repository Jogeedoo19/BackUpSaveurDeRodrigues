using Microsoft.EntityFrameworkCore;
using sdrproj.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Merchant> Merchants { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<SubCategory> SubCategories { get; set; }
    public DbSet<Product> Products { get; set; }

    public DbSet<ContactUs> ContactUs { get; set; }

    public DbSet<WishList> WishList { get; set; }
    public DbSet<Cart> Carts { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2); 
    }

}

