using FlexibleCatalogPoc.Models;
using FlexibleCatalogPoc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlexibleCatalogPoc.Pages;

public class IndexModel : PageModel
{
    private readonly ProductService _products;

    public IndexModel(ProductService products)
    {
        _products = products;
    }

    [BindProperty(SupportsGet = true)]
    public string? Category { get; set; }

    public IReadOnlyList<Product> Products { get; private set; } = [];
    public string? SetupError { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Products = await _products.ListAsync(Category, cancellationToken);
        }
        catch (Exception ex)
        {
            SetupError = ex.Message;
        }
    }
}
