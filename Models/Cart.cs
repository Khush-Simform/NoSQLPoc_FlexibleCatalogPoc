using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FlexibleCatalogPoc.Models;

[BsonIgnoreExtraElements]
public class Cart
{
    public const string DemoUserId = "demo-user";
    public const string StatusActive = "Active";
    public const string StatusCheckingOut = "CheckingOut";
    public const string StatusPaid = "Paid";

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string UserId { get; set; } = DemoUserId;
    public string Status { get; set; } = StatusActive;
    public List<CartItem> Items { get; set; } = [];
    public PromoSnapshot? Promo { get; set; }
    public CartTotals Totals { get; set; } = new();
    public PaymentDemo? Payment { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class CartItem
{
    public string ProductId { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Qty { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal UnitPrice { get; set; }

    public BsonDocument AttributeSnapshot { get; set; } = [];
}

public class PromoSnapshot
{
    public string Code { get; set; } = string.Empty;
    public int PercentOff { get; set; }
}

public class CartTotals
{
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Subtotal { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Discount { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Total { get; set; }
}

public class PaymentDemo
{
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public List<string> Timeline { get; set; } = [];
    public DateTime At { get; set; } = DateTime.UtcNow;
}
