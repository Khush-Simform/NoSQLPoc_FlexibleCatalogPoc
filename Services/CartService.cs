using FlexibleCatalogPoc.Data;
using FlexibleCatalogPoc.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FlexibleCatalogPoc.Services;

public class CartService
{
    public const string DemoPromoCode = "SAVE10";

    private readonly MongoContext _mongo;
    private readonly ProductService _products;

    public CartService(MongoContext mongo, ProductService products)
    {
        _mongo = mongo;
        _products = products;
    }

    public async Task<Cart> GetOrCreateDemoCartAsync(CancellationToken cancellationToken = default)
    {
        var cart = await _mongo.Carts.Find(c => c.UserId == Cart.DemoUserId).FirstOrDefaultAsync(cancellationToken);
        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart { UserId = Cart.DemoUserId };
        Recalculate(cart);
        await _mongo.Carts.InsertOneAsync(cart, cancellationToken: cancellationToken);
        return cart;
    }

    public async Task<Cart> AddItemAsync(AddCartItemRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Qty < 1)
        {
            throw new InvalidOperationException("Quantity must be at least 1.");
        }

        var product = await _products.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new InvalidOperationException("Product not found.");

        var cart = await GetOrCreateDemoCartAsync(cancellationToken);
        PrepareForEdits(cart);

        var sku = string.IsNullOrWhiteSpace(request.Sku) ? product.Sku : request.Sku.Trim();
        var variant = product.Variants.FirstOrDefault(v => v.Sku == sku);
        if (variant is null && sku != product.Sku)
        {
            throw new InvalidOperationException($"SKU '{sku}' is not a variant of {product.Name}.");
        }

        var unitPrice = variant?.Price ?? product.Price;
        var snapshot = BuildSnapshot(product, variant);
        var existing = cart.Items.FirstOrDefault(i => i.Sku == sku);
        if (existing is null)
        {
            cart.Items.Add(new CartItem
            {
                ProductId = product.Id!,
                Sku = sku,
                Name = product.Name,
                Qty = request.Qty,
                UnitPrice = unitPrice,
                AttributeSnapshot = snapshot
            });
        }
        else
        {
            existing.Qty += request.Qty;
            existing.UnitPrice = unitPrice;
            existing.AttributeSnapshot = snapshot;
        }

        Recalculate(cart);
        await ReplaceAsync(cart, cancellationToken);
        return cart;
    }

    public async Task<Cart> RemoveItemAsync(string sku, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateDemoCartAsync(cancellationToken);
        PrepareForEdits(cart);
        cart.Items.RemoveAll(i => i.Sku == sku);
        Recalculate(cart);
        await ReplaceAsync(cart, cancellationToken);
        return cart;
    }

    public async Task<Cart> ApplyPromoAsync(string code, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateDemoCartAsync(cancellationToken);
        PrepareForEdits(cart);

        var normalized = code.Trim().ToUpperInvariant();
        if (normalized != DemoPromoCode)
        {
            throw new InvalidOperationException($"Promo '{code}' is not valid in this demo. Try {DemoPromoCode}.");
        }

        cart.Promo = new PromoSnapshot { Code = DemoPromoCode, PercentOff = 10 };
        Recalculate(cart);
        await ReplaceAsync(cart, cancellationToken);
        return cart;
    }

    public async Task<Cart> CheckoutAsync(CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateDemoCartAsync(cancellationToken);
        if (cart.Items.Count == 0)
        {
            throw new InvalidOperationException("Cart is empty.");
        }

        if (cart.Status == Cart.StatusPaid)
        {
            throw new InvalidOperationException("This cart is already paid. Add a new item to start another demo order.");
        }

        Recalculate(cart);
        cart.Status = Cart.StatusCheckingOut;
        cart.Payment = new PaymentDemo
        {
            Status = "Processing",
            Message = "Payment processing...",
            Timeline = ["Payment processing..."],
            At = DateTime.UtcNow
        };
        await ReplaceAsync(cart, cancellationToken);

        // POC only: no Stripe/PayPal. The same cart document is flipped to Paid.
        cart.Status = Cart.StatusPaid;
        cart.Payment = new PaymentDemo
        {
            Status = "Paid",
            Message = "Payment done",
            TransactionId = $"DEMO-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Timeline = ["Payment processing...", "Payment done"],
            At = DateTime.UtcNow
        };
        await ReplaceAsync(cart, cancellationToken);
        return cart;
    }

    private static void PrepareForEdits(Cart cart)
    {
        if (cart.Status != Cart.StatusPaid)
        {
            return;
        }

        cart.Status = Cart.StatusActive;
        cart.Payment = null;
    }

    private static BsonDocument BuildSnapshot(Product product, ProductVariant? variant)
    {
        var snapshot = product.Attributes.DeepClone().AsBsonDocument;
        snapshot["category"] = product.Category;
        if (product.Brand is not null)
        {
            snapshot["brand"] = product.Brand;
        }

        if (variant is not null)
        {
            foreach (var element in variant.Extra)
            {
                snapshot[element.Name] = element.Value;
            }
        }

        return snapshot;
    }

    private static void Recalculate(Cart cart)
    {
        var subtotal = cart.Items.Sum(i => i.UnitPrice * i.Qty);
        var discount = cart.Promo?.Code == DemoPromoCode
            ? Math.Round(subtotal * cart.Promo.PercentOff / 100m, 2, MidpointRounding.AwayFromZero)
            : 0m;

        cart.Totals = new CartTotals
        {
            Subtotal = subtotal,
            Discount = discount,
            Total = subtotal - discount
        };
        cart.UpdatedAt = DateTime.UtcNow;
    }

    private Task ReplaceAsync(Cart cart, CancellationToken cancellationToken) =>
        _mongo.Carts.ReplaceOneAsync(c => c.Id == cart.Id, cart, cancellationToken: cancellationToken);
}
