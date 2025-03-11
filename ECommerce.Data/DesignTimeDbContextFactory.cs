using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ECommerce.Data.Context;
using Microsoft.Extensions.Configuration;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer("Server=DESKTOP-3LKJAP1\\SQLEXPRESS;Database=ECommerceDB;User Id=sa;Password=1234;TrustServerCertificate=True;");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
} 


