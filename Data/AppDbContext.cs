using Microsoft.EntityFrameworkCore;
using JobForgeAPI.Models;

namespace JobForgeAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Job> Jobs { get; set; }
}