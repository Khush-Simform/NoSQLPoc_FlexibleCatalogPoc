using FlexibleCatalogPoc.Models;
using FlexibleCatalogPoc.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlexibleCatalogPoc.Controllers;

[ApiController]
[Route("api/carts")]
public class CartsController(CartService carts) : ControllerBase
{
    [HttpGet("demo")]
    public async Task<ActionResult<Cart>> GetDemo(CancellationToken cancellationToken)
    {
        return await carts.GetOrCreateDemoCartAsync(cancellationToken);
    }

    [HttpPost("demo/items")]
    public async Task<ActionResult<Cart>> AddItem([FromBody] AddCartItemRequest request, CancellationToken cancellationToken)
    {
        return await Execute(() => carts.AddItemAsync(request, cancellationToken));
    }

    [HttpDelete("demo/items/{sku}")]
    public async Task<ActionResult<Cart>> RemoveItem(string sku, CancellationToken cancellationToken)
    {
        return await Execute(() => carts.RemoveItemAsync(sku, cancellationToken));
    }

    [HttpPost("demo/promo")]
    public async Task<ActionResult<Cart>> ApplyPromo([FromBody] ApplyPromoRequest request, CancellationToken cancellationToken)
    {
        return await Execute(() => carts.ApplyPromoAsync(request.Code, cancellationToken));
    }

    [HttpPost("demo/checkout")]
    public async Task<ActionResult<Cart>> Checkout(CancellationToken cancellationToken)
    {
        return await Execute(() => carts.CheckoutAsync(cancellationToken));
    }

    private async Task<ActionResult<Cart>> Execute(Func<Task<Cart>> action)
    {
        try
        {
            return await action();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
