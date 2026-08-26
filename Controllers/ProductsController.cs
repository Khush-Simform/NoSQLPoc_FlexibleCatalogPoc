using FlexibleCatalogPoc.Models;
using FlexibleCatalogPoc.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlexibleCatalogPoc.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _products;

    public ProductsController(ProductService products)
    {
        _products = products;
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> List([FromQuery] string? category, CancellationToken cancellationToken)
    {
        return await _products.ListAsync(category, cancellationToken);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<Product>>> Search(
        [FromQuery] string? q,
        [FromQuery] string? category,
        [FromQuery] int? storageGb,
        [FromQuery] int? ramGb,
        [FromQuery] string? fabric,
        [FromQuery] string? material,
        CancellationToken cancellationToken)
    {
        return await _products.SearchAsync(q, category, storageGb, ramGb, fabric, material, cancellationToken);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(string id, CancellationToken cancellationToken)
    {
        var product = await _products.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : product;
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _products.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/reviews")]
    public async Task<ActionResult<Product>> AddReview(string id, [FromBody] AddReviewRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _products.AddReviewAsync(id, request, cancellationToken);
            return product is null ? NotFound() : product;
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
