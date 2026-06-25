using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHotelBackend.Data;
using SmartHotelBackend.Models;

namespace SmartHotelBackend.Controllers;

[ApiController]
[Route("api/language-preference")]
public class LanguagePreferenceController : ControllerBase
{
    private readonly SmartHotelContext _db;

    public LanguagePreferenceController(SmartHotelContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Save or update the language preference for a session.
    /// Called when the user clicks "Continue" on the Language Selection page.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SavePreference([FromBody] SaveLanguageRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.SessionId) || string.IsNullOrWhiteSpace(req.LanguageCode))
            return BadRequest(new { error = "sessionId and languageCode are required" });

        var existing = await _db.SessionLanguagePreferences
            .FirstOrDefaultAsync(p => p.SessionId == req.SessionId);

        if (existing != null)
        {
            existing.LanguageCode = req.LanguageCode;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _db.SessionLanguagePreferences.Add(new SessionLanguagePreference
            {
                SessionId    = req.SessionId,
                LanguageCode = req.LanguageCode,
            });
        }

        await _db.SaveChangesAsync();
        return Ok(new { sessionId = req.SessionId, languageCode = req.LanguageCode, saved = true });
    }

    /// <summary>
    /// Retrieve the language preference for a session.
    /// </summary>
    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetPreference(string sessionId)
    {
        var pref = await _db.SessionLanguagePreferences
            .FirstOrDefaultAsync(p => p.SessionId == sessionId);

        if (pref == null)
            return NotFound(new { error = "No preference found" });

        return Ok(new { sessionId = pref.SessionId, languageCode = pref.LanguageCode });
    }
}

public record SaveLanguageRequest(string SessionId, string LanguageCode);
