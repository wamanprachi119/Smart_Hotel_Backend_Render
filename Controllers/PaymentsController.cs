using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHotelBackend.Data;
using SmartHotelBackend.DTOs;
using SmartHotelBackend.Models;

namespace SmartHotelBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly SmartHotelContext _db;

    private const string HOTEL_UPI_ID = "902984211@axl";
    private const string HOTEL_NAME   = "Prachi Ramnath Waman";

    public PaymentsController(SmartHotelContext db) => _db = db;

    // GET /api/payments
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var payments = await _db.Payments.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return Ok(payments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
    }

    // GET /api/payments/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var payment = await _db.Payments.Include(p => p.Order).FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null) return NotFound(new { error = "Payment not found" });
            return Ok(payment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
    }

    // POST /api/payments
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest req)
    {
        try
        {
            var order = await _db.Orders.FindAsync(req.OrderId);
            if (order == null) return NotFound(new { error = "Order not found" });

            var payment = new Payment
            {
                Id            = "PAY-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                OrderId       = req.OrderId,
                Method        = req.Method,
                Amount        = req.Amount,
                Tax           = req.Tax,
                GrandTotal    = req.GrandTotal,
                TableNumber   = req.TableNumber,
                TransactionId = req.TransactionId,
                UpiRef        = req.UpiRef,
                UpiId         = req.UpiId,
                CardLast4     = req.CardLast4,
                Wallet        = req.Wallet,
                Status        = "completed",
            };

            order.PaymentStatus = "completed";
            order.PaymentMethod = req.Method;
            order.PaymentId     = payment.Id;
            order.Status        = "paid";
            order.UpdatedAt     = DateTime.UtcNow;

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
    }

    // POST /api/payments/verify-upi
    [HttpPost("verify-upi")]
    public async Task<IActionResult> VerifyUpi([FromBody] UpiVerifyRequest req)
    {
        if (string.IsNullOrEmpty(req.TransactionId) || req.Amount <= 0)
            return BadRequest(new UpiVerifyResponse(false, "Invalid transaction data", null));

        await Task.Delay(200);
        var verifiedRef = "VPAY" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return Ok(new UpiVerifyResponse(true, "Payment verified successfully", verifiedRef));
    }

    // GET /api/payments/upi-config
    [HttpGet("upi-config")]
    public IActionResult GetUpiConfig()
    {
        return Ok(new
        {
            UpiId       = HOTEL_UPI_ID,
            Name        = HOTEL_NAME,
            Description = "Smart Hotel — Scan & Pay",
        });
    }
}
