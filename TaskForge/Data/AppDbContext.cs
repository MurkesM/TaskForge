using Microsoft.EntityFrameworkCore;
using TaskForge.Api.Models;
using TaskForge.Models;

namespace TaskForge.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
}