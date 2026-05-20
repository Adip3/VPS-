using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VPS.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var opt = new DbContextOptionsBuilder<AppDbContext>();
        opt.UseNpgsql("Host=localhost;Port=5432;Database=vps_db;Username=postgres;Password=postgres");
        return new AppDbContext(opt.Options);
    }
}
