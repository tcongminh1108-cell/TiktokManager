using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.Products;
using TikTokShop.Application.Features.Products.Dtos;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController(
    IProductService productService,
    IValidator<CreateProductRequest> createValidator,
    IValidator<UpdateProductRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<ProductDto>>>> GetProducts(
        [FromQuery] ProductQueryParams query)
    {
        var result = await productService.GetProductsAsync(query);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(Guid id)
    {
        var result = await productService.GetProductByIdAsync(id);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "RequireManagerOrAbove")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct(
        [FromBody] CreateProductRequest request)
    {
        await createValidator.ValidateAndThrowAsync(request);
        var result = await productService.CreateProductAsync(request);
        return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, ApiResponse.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireManagerOrAbove")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(
        Guid id, [FromBody] UpdateProductRequest request)
    {
        await updateValidator.ValidateAndThrowAsync(request);
        var result = await productService.UpdateProductAsync(id, request);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireManagerOrAbove")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(Guid id)
    {
        await productService.DeleteProductAsync(id);
        return Ok(ApiResponse.Ok<object>(null!, "Product deleted."));
    }

    [HttpPost("{id:guid}/restore")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> RestoreProduct(Guid id)
    {
        await productService.RestoreProductAsync(id);
        return Ok(ApiResponse.Ok<object>(null!, "Product restored."));
    }

    [HttpPut("{id:guid}/activate")]
    [Authorize(Policy = "RequireManagerOrAbove")]
    public async Task<ActionResult<ApiResponse<object>>> ActivateProduct(Guid id)
    {
        await productService.ActivateProductAsync(id);
        return Ok(ApiResponse.Ok<object>(null!, "Product activated."));
    }

    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Policy = "RequireManagerOrAbove")]
    public async Task<ActionResult<ApiResponse<object>>> DeactivateProduct(Guid id)
    {
        await productService.DeactivateProductAsync(id);
        return Ok(ApiResponse.Ok<object>(null!, "Product deactivated."));
    }
}
