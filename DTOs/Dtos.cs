namespace SmartHotelBackend.DTOs;

// ── Orders ────────────────────────────────────────────────────
public record CreateOrderRequest(
    string TableNumber,
    List<OrderItemDto> Items
);

public record OrderItemDto(
    string ItemId,
    string Name,
    decimal Price,
    int Quantity,
    string? ImageUrl,
    int PrepTime,
    string? TableNumber
);

public record UpdateOrderStatusRequest(string Status);

// ── Payment ───────────────────────────────────────────────────
public record CreatePaymentRequest(
    string OrderId,
    string Method,
    decimal Amount,
    decimal Tax,
    decimal GrandTotal,
    string? TableNumber,
    string? TransactionId,
    string? UpiRef,
    string? UpiId,
    string? CardLast4,
    string? Wallet
);

// ── Feedback ──────────────────────────────────────────────────
public record CreateFeedbackRequest(
    string OrderId,
    string? TableNumber,
    int FoodRating,
    int ServiceRating,
    int AmbianceRating,
    string? Comment
);

// ── UPI Verify ────────────────────────────────────────────────
public record UpiVerifyRequest(
    string TransactionId,
    string UpiRef,
    decimal Amount
);

public record UpiVerifyResponse(
    bool Success,
    string? Message,
    string? VerifiedRef
);

// ── Cart ──────────────────────────────────────────────────────
public record UpsertCartRequest(
    string SessionId,
    string TableNumber,
    List<CartItemDto> Items
);

public record CartItemDto(
    string ItemId,
    string Name,
    decimal Price,
    int Quantity,
    string? ImageUrl,
    int PrepTime
);

public record AddCartItemRequest(
    string ItemId,
    string Name,
    decimal Price,
    int Quantity,
    string? ImageUrl,
    int PrepTime
);

public record UpdateCartItemRequest(int Quantity);
