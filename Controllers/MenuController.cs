using Microsoft.AspNetCore.Mvc;

namespace SmartHotelBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    // Menu is served from static data (frontend has menuData.js).
    // This endpoint can be used for future DB-backed menu management.

    [HttpGet]
    public IActionResult GetMenu()
    {
        return Ok(new { message = "Menu is served from frontend static data.", status = "ok" });
    }

    [HttpGet("categories")]
    public IActionResult GetCategories()
    {
        var categories = new[]
        {
            "Indian", "Punjabi", "South Indian", "Chinese", "Italian",
            "Fast Food", "Breakfast", "Veg", "Non-Veg", "Starter",
            "Main Course", "Soup", "Salad", "Bread", "Healthy Food",
            "Kids Special", "Desserts", "Beverages", "Drinks"
        };
        return Ok(categories);
    }
}
