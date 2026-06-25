using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHotelBackend.Data;

namespace SmartHotelBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly SmartHotelContext _db;
    public NotificationsController(SmartHotelContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] string? tableNumber)
    {
        try
        {
            var query = _db.Orders.AsQueryable();
            if (!string.IsNullOrEmpty(tableNumber))
                query = query.Where(o => o.TableNumber == tableNumber);

            var recentOrders = await query
                .OrderByDescending(o => o.UpdatedAt)
                .Take(20)
                .ToListAsync();

            var notifications = recentOrders.Select(o => new
            {
                id          = o.Id,
                orderId     = o.Id,
                tableNumber = o.TableNumber,
                message     = GetNotificationMessage(o.Status),
                status      = o.Status,
                timestamp   = o.UpdatedAt,
            });

            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
    }

    private static string GetNotificationMessage(string status) => status switch
    {
        "pending"   => "Your order has been received.",
        "accepted"  => "Your order has been accepted by the kitchen.",
        "preparing" => "Your food is being prepared.",
        "ready"     => "Your order is ready! It will be served shortly.",
        "served"    => "Enjoy your meal! 🍽️",
        "paid"      => "Payment received. Thank you!",
        _           => "Order status updated."
    };
}
