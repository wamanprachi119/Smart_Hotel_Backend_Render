using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHotelBackend.Data;
using SmartHotelBackend.DTOs;
using SmartHotelBackend.Models;

namespace SmartHotelBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly SmartHotelContext _db;
    public OrdersController(SmartHotelContext db) => _db = db;

    // GET /api/orders
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? tableNumber)
    {
        try
        {
            var query = _db.Orders.Include(o => o.Items).AsQueryable();
            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);
            if (!string.IsNullOrEmpty(tableNumber))
                query = query.Where(o => o.TableNumber == tableNumber);
            var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
    }

    // GET /api/orders/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var order = await _db.Orders.Include(o => o.Items).Include(o => o.Payment)
                                 .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound(new { error = "Order not found" });
            return Ok(order);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
    }

    // POST /api/orders
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest req)
    {
        try
        {
            var order = new Order
            {
                Id            = "ORD-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                TableNumber   = req.TableNumber,
                Status        = "pending",
                PaymentStatus = "pending",
                Total         = req.Items.Sum(i => i.Price * i.Quantity),
                Items         = req.Items.Select(i => new OrderItem
                {
                    ItemId      = i.ItemId,
                    Name        = i.Name,
                    Price       = i.Price,
                    Quantity    = i.Quantity,
                    ImageUrl    = i.ImageUrl,
                    PrepTime    = i.PrepTime,
                    TableNumber = i.TableNumber,
                }).ToList(),
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
    }

    // PATCH /api/orders/{id}/status
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateOrderStatusRequest req)
    {
        try
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return NotFound(new { error = "Order not found" });

            var allowed = new[] { "pending", "accepted", "preparing", "ready", "served", "paid" };
            if (!allowed.Contains(req.Status))
                return BadRequest(new { error = "Invalid status value" });

            order.Status    = req.Status;
            order.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(order);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
    }

    // DELETE /api/orders/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return NotFound(new { error = "Order not found" });
            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
    }
}
