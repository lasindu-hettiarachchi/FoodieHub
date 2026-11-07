
using FoodieHub.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodieHub.API.Data;

public class FoodieHubDbContext : DbContext
{
    public FoodieHubDbContext(
        DbContextOptions<FoodieHubDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
}
