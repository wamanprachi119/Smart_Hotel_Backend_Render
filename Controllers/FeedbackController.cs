using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHotelBackend.Data;
using SmartHotelBackend.DTOs;
using SmartHotelBackend.Models;

namespace SmartHotelBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly SmartHotelContext _db;
    public FeedbackController(SmartHotelContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var feedbacks = await _db.Feedbacks.OrderByDescending(f => f.CreatedAt).ToListAsync();
            return Ok(feedbacks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeedbackRequest req)
    {
        try
        {
            var feedback = new Feedback
            {
                OrderId        = req.OrderId,
                TableNumber    = req.TableNumber,
                FoodRating     = req.FoodRating,
                ServiceRating  = req.ServiceRating,
                AmbianceRating = req.AmbianceRating,
                Comment        = req.Comment,
            };
            _db.Feedbacks.Add(feedback);
            await _db.SaveChangesAsync();
            return Ok(feedback);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
    }
}
