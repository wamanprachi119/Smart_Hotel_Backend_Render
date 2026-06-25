using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHotelBackend.Data;
using SmartHotelBackend.DTOs;
using SmartHotelBackend.Models;

namespace SmartHotelBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly SmartHotelContext _db;
    public CartController(SmartHotelContext db) => _db = db;

    // GET /api/cart/{sessionId}
    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetCart(string sessionId)
    {
        try
        {
            var cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (cart == null)
                return Ok(new { sessionId, tableNumber = "", items = new List<object>(), total = 0 });

            return Ok(MapCart(cart));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
    }

    // PUT /api/cart
    [HttpPut]
    public async Task<IActionResult> UpsertCart([FromBody] UpsertCartRequest req)
    {
        try
        {
            var cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == req.SessionId);

            if (cart == null)
            {
                cart = new Cart { SessionId = req.SessionId, TableNumber = req.TableNumber };
                _db.Carts.Add(cart);
            }
            else
            {
                cart.TableNumber = req.TableNumber;
                cart.UpdatedAt   = DateTime.UtcNow;
                _db.CartItems.RemoveRange(cart.Items);
                cart.Items.Clear();
            }

            cart.Items = req.Items.Select(i => new CartItem
            {
                ItemId   = i.ItemId,
                Name     = i.Name,
                Price    = i.Price,
                Quantity = i.Quantity,
                ImageUrl = i.ImageUrl,
                PrepTime = i.PrepTime,
            }).ToList();

            await _db.SaveChangesAsync();
            return Ok(MapCart(cart));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
    }

    // POST /api/cart/{sessionId}/items
    [HttpPost("{sessionId}/items")]
    public async Task<IActionResult> AddItem(string sessionId, [FromBody] AddCartItemRequest req)
    {
        try
        {
            var cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (cart == null)
            {
                cart = new Cart { SessionId = sessionId };
                _db.Carts.Add(cart);
            }

            var existing = cart.Items.FirstOrDefault(i => i.ItemId == req.ItemId);
            if (existing != null)
                existing.Quantity += req.Quantity;
            else
                cart.Items.Add(new CartItem
                {
                    ItemId   = req.ItemId,
                    Name     = req.Name,
                    Price    = req.Price,
                    Quantity = req.Quantity,
                    ImageUrl = req.ImageUrl,
                    PrepTime = req.PrepTime,
                });

            cart.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(MapCart(cart));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
    }

    // PATCH /api/cart/{sessionId}/items/{itemId}
    [HttpPatch("{sessionId}/items/{itemId}")]
    public async Task<IActionResult> UpdateItem(string sessionId, string itemId, [FromBody] UpdateCartItemRequest req)
    {
        try
        {
            var cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (cart == null) return NotFound(new { error = "Cart not found" });

            var item = cart.Items.FirstOrDefault(i => i.ItemId == itemId);
            if (item == null) return NotFound(new { error = "Item not found in cart" });

            if (req.Quantity <= 0)
                _db.CartItems.Remove(item);
            else
                item.Quantity = req.Quantity;

            cart.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(MapCart(cart));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
    }

    // DELETE /api/cart/{sessionId}/items/{itemId}
    [HttpDelete("{sessionId}/items/{itemId}")]
    public async Task<IActionResult> RemoveItem(string sessionId, string itemId)
    {
        try
        {
            var cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (cart == null) return NotFound(new { error = "Cart not found" });

            var item = cart.Items.FirstOrDefault(i => i.ItemId == itemId);
            if (item != null) _db.CartItems.Remove(item);

            cart.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(MapCart(cart));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
    }

    // DELETE /api/cart/{sessionId}
    [HttpDelete("{sessionId}")]
    public async Task<IActionResult> ClearCart(string sessionId)
    {
        try
        {
            var cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (cart != null)
            {
                _db.CartItems.RemoveRange(cart.Items);
                cart.Items.Clear();
                cart.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            return Ok(new { message = "Cart cleared", sessionId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
    }

    private static object MapCart(Cart cart) => new
    {
        id          = cart.Id,
        sessionId   = cart.SessionId,
        tableNumber = cart.TableNumber,
        updatedAt   = cart.UpdatedAt,
        items       = cart.Items.Select(i => new
        {
            id       = i.Id,
            itemId   = i.ItemId,
            name     = i.Name,
            price    = i.Price,
            quantity = i.Quantity,
            imageUrl = i.ImageUrl,
            prepTime = i.PrepTime,
            subtotal = i.Price * i.Quantity,
        }),
        total     = cart.Items.Sum(i => i.Price * i.Quantity),
        itemCount = cart.Items.Sum(i => i.Quantity),
    };
}
