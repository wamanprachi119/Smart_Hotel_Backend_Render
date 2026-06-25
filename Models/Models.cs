using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHotelBackend.Models;

public class Order
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TableNumber { get; set; } = "";
    public string Status { get; set; } = "pending"; // pending | accepted | preparing | ready | served | paid
    public string PaymentStatus { get; set; } = "pending"; // pending | completed
    public string? PaymentMethod { get; set; }
    public string? PaymentId { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public List<OrderItem> Items { get; set; } = new();
    public Payment? Payment { get; set; }
}

public class OrderItem
{
    [Key]
    public int Id { get; set; }
    public string OrderId { get; set; } = "";
    public string ItemId { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public int PrepTime { get; set; }
    public string? TableNumber { get; set; }

    [ForeignKey("OrderId")]
    public Order? Order { get; set; }
}

public class Payment
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OrderId { get; set; } = "";
    public string Method { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal Tax { get; set; }
    public decimal GrandTotal { get; set; }
    public string Status { get; set; } = "completed";
    public string? TransactionId { get; set; }
    public string? UpiRef { get; set; }
    public string? UpiId { get; set; }
    public string? CardLast4 { get; set; }
    public string? Wallet { get; set; }
    public string? TableNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("OrderId")]
    public Order? Order { get; set; }
}

public class Feedback
{
    [Key]
    public int Id { get; set; }
    public string OrderId { get; set; } = "";
    public string? TableNumber { get; set; }
    public int FoodRating { get; set; }
    public int ServiceRating { get; set; }
    public int AmbianceRating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ── Cart (stored per session/table in DB) ──────────────────────
public class Cart
{
    [Key]
    public int Id { get; set; }
    public string SessionId { get; set; } = "";   // tableNumber or browser session id
    public string TableNumber { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<CartItem> Items { get; set; } = new();
}

public class CartItem
{
    [Key]
    public int Id { get; set; }
    public int CartId { get; set; }
    public string ItemId { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public int PrepTime { get; set; }

    [ForeignKey("CartId")]
    public Cart? Cart { get; set; }
}
