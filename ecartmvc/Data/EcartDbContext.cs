using ecartmvc.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace ecartmvc.Data;

public partial class EcartDbContext : DbContext
{
    public EcartDbContext()
    {
    }

    public EcartDbContext(DbContextOptions<EcartDbContext> options): base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);

        // Reviews -> Product (many-to-one)
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Product)
            .WithMany(p => p.Reviews)   
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Review> Reviews { get; set; }
}
