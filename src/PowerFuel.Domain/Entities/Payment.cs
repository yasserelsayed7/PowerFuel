namespace Domain.Entities;

public class Payment
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string StripePaymentIntentId { get; private set; } = default!;
    public string ClientSecret { get; private set; } = default!;
    public long Amount { get; private set; }           // in cents
    public string Currency { get; private set; } = default!;
    public string Status { get; private set; } = default!;
    public string? Description { get; private set; }
    public string? CustomerEmail { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    private Payment() { }   // EF / deserialization

    public static Payment Create(
        string stripePaymentIntentId,
        string clientSecret,
        long amount,
        string currency,
        string? description,
        string? customerEmail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stripePaymentIntentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientSecret);
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be > 0");

        return new Payment
        {
            StripePaymentIntentId = stripePaymentIntentId,
            ClientSecret          = clientSecret,
            Amount                = amount,
            Currency              = currency.ToLowerInvariant(),
            Status                = "requires_payment_method",
            Description           = description,
            CustomerEmail         = customerEmail
        };
    }

    public void UpdateStatus(string newStatus)
    {
        Status    = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }
}
