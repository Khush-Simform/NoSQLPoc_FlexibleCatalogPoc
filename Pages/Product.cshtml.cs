using FlexibleCatalogPoc.Models;
using FlexibleCatalogPoc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlexibleCatalogPoc.Pages;

public class ProductModel : PageModel
{
    private readonly ProductService _products;
    private readonly CartService _carts;

    public ProductModel(ProductService products, CartService carts)
    {
        _products = products;
        _carts = carts;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    [BindProperty]
    public string Sku { get; set; } = string.Empty;

    [BindProperty]
    public int Qty { get; set; } = 1;

    [BindProperty]
    public string Author { get; set; } = string.Empty;

    [BindProperty]
    public int Rating { get; set; } = 5;

    [BindProperty]
    public string Comment { get; set; } = string.Empty;

    public Product? Product { get; private set; }
    public string? Error { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
        if (Product is null && Error is null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAddToCartAsync(CancellationToken cancellationToken)
    {
        if (!await LoadAsync(cancellationToken) || Product is null)
        {
            return NotFound();
        }

        try
        {
            await _carts.AddItemAsync(new AddCartItemRequest
            {
                ProductId = Product.Id!,
                Sku = string.IsNullOrWhiteSpace(Sku) ? Product.Sku : Sku,
                Qty = Qty
            }, cancellationToken);
            return RedirectToPage("/Cart");
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAddReviewAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _products.AddReviewAsync(Id, new AddReviewRequest
            {
                Author = Author,
                Rating = Rating,
                Comment = Comment
            }, cancellationToken);
            return RedirectToPage(new { id = Id });
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    private async Task<bool> LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            Product = await _products.GetByIdAsync(Id, cancellationToken);
            return Product is not null;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            return false;
        }
    }
}
