using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.Suppliers;
using TikTokShop.Application.Features.Suppliers.Dtos;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
[Authorize]
public class SuppliersController(
    ISupplierService supplierService,
    IValidator<CreateSupplierRequest> createValidator,
    IValidator<UpdateSupplierRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<SupplierDto>>>> GetSuppliers(
        [FromQuery] SupplierQueryParams query)
    {
        var result = await supplierService.GetSuppliersAsync(query);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> GetSupplier(Guid id)
    {
        var result = await supplierService.GetSupplierByIdAsync(id);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "RequireManagerOrAbove")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> CreateSupplier(
        [FromBody] CreateSupplierRequest request)
    {
        await createValidator.ValidateAndThrowAsync(request);
        var result = await supplierService.CreateSupplierAsync(request);
        return CreatedAtAction(nameof(GetSupplier), new { id = result.Id }, ApiResponse.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireManagerOrAbove")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> UpdateSupplier(
        Guid id, [FromBody] UpdateSupplierRequest request)
    {
        await updateValidator.ValidateAndThrowAsync(request);
        var result = await supplierService.UpdateSupplierAsync(id, request);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireManagerOrAbove")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSupplier(Guid id)
    {
        await supplierService.DeleteSupplierAsync(id);
        return Ok(ApiResponse.Ok<object>(null!, "Supplier deleted."));
    }

    [HttpPost("{id:guid}/restore")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> RestoreSupplier(Guid id)
    {
        await supplierService.RestoreSupplierAsync(id);
        return Ok(ApiResponse.Ok<object>(null!, "Supplier restored."));
    }
}
