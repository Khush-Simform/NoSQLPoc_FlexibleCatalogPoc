using FlexibleCatalogPoc.Models;
using FlexibleCatalogPoc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlexibleCatalogPoc.Pages;

public class CartPageModel : PageModel
{
    private readonly CartService _carts;

    public CartPageModel(CartService carts)
    {
        _carts = carts;
    }

    public Cart? Cart { get; private set; }
    public string? Error { get; private set; }

    [BindProperty]
    public string PromoCode { get; set; } = CartService.DemoPromoCode;

    public async Task OnGetAsync(CancellationToken cancellationToken) => await LoadAsync(cancellationToken);

    public async Task<IActionResult> OnPostRemoveAsync(string sku, CancellationToken cancellationToken)
    {
        return await Run(() => _carts.RemoveItemAsync(sku, cancellationToken), cancellationToken);
    }

    public async Task<IActionResult> OnPostPromoAsync(CancellationToken cancellationToken)
    {
        return await Run(() => _carts.ApplyPromoAsync(PromoCode, cancellationToken), cancellationToken);
    }

    public async Task<IActionResult> OnPostCheckoutAsync(CancellationToken cancellationToken)
    {
        return await Run(() => _carts.CheckoutAsync(cancellationToken), cancellationToken);
    }

    private async Task<IActionResult> Run(Func<Task<Cart>> action, CancellationToken cancellationToken)
    {
        try
        {
            Cart = await action();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            await LoadAsync(cancellationToken);
        }

        return Page();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            Cart = await _carts.GetOrCreateDemoCartAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
}
