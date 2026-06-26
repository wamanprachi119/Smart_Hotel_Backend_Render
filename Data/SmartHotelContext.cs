using Microsoft.EntityFrameworkCore;
using SmartHotelBackend.Models;

namespace SmartHotelBackend.Data;

public class SmartHotelContext : DbContext
{
    public SmartHotelContext(DbContextOptions<SmartHotelContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<SessionLanguagePreference> SessionLanguagePreferences => Set<SessionLanguagePreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================
        // TABLE NAMES (Railway MySQL)
        // ==========================

        modelBuilder.Entity<Order>().ToTable("orders");
        modelBuilder.Entity<OrderItem>().ToTable("orderitems");
        modelBuilder.Entity<Payment>().ToTable("payments");
        modelBuilder.Entity<Feedback>().ToTable("feedbacks");
        modelBuilder.Entity<Cart>().ToTable("carts");
        modelBuilder.Entity<CartItem>().ToTable("cartitems");
        modelBuilder.Entity<SessionLanguagePreference>().ToTable("sessionlanguagepreferences");

        // ==========================
        // RELATIONSHIPS
        // ==========================

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Payment)
            .WithOne(p => p.Order)
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Cart>()
            .HasMany(c => c.Items)
            .WithOne(i => i.Cart)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==========================
        // DECIMAL PRECISION
        // ==========================

        modelBuilder.Entity<Order>()
            .Property(o => o.Total)
            .HasPrecision(10, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(i => i.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Tax)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.GrandTotal)
            .HasPrecision(10, 2);

        modelBuilder.Entity<CartItem>()
            .Property(i => i.Price)
            .HasPrecision(10, 2);

        // ==========================
        // INDEXES
        // ==========================

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Status);

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.TableNumber);

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.CreatedAt);

        modelBuilder.Entity<Cart>()
            .HasIndex(c => c.SessionId);

        modelBuilder.Entity<Cart>()
            .HasIndex(c => c.TableNumber);

        modelBuilder.Entity<SessionLanguagePreference>()
            .HasIndex(s => s.SessionId);
    }
}