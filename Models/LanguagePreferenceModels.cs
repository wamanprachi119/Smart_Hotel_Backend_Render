using System.ComponentModel.DataAnnotations;

namespace SmartHotelBackend.Models;

/// <summary>
/// Stores the language preference for each browser session.
/// Created/updated whenever a user completes language selection.
/// </summary>
public class SessionLanguagePreference
{
    [Key]
    public int Id { get; set; }

    /// <summary>Browser session ID (from localStorage smartHotelSessionId)</summary>
    [Required, MaxLength(100)]
    public string SessionId { get; set; } = "";

    /// <summary>Selected language code: en | hi | mr</summary>
    [Required, MaxLength(10)]
    public string LanguageCode { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
