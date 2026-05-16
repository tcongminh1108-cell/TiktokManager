using FluentAssertions;
using TikTokShop.Application.Features.StockIns.Dtos;
using TikTokShop.Application.Features.StockIns.Validators;

namespace TikTokShop.UnitTests.Features.StockIns.Validators;

public class CreateStockInRequestValidatorTests
{
    private readonly CreateStockInRequestValidator _sut = new();

    private static CreateStockInRequest ValidRequest() => new(
        ProductId: Guid.NewGuid(),
        SupplierId: Guid.NewGuid(),
        Quantity: 10,
        UnitPrice: 50m,
        TransactionDate: DateTimeOffset.UtcNow,
        Note: null);

    [Fact]
    public void Valid_request_passes()
    {
        _sut.Validate(ValidRequest()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_product_id_fails()
    {
        var result = _sut.Validate(ValidRequest() with { ProductId = Guid.Empty });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateStockInRequest.ProductId));
    }

    [Fact]
    public void Empty_supplier_id_fails()
    {
        var result = _sut.Validate(ValidRequest() with { SupplierId = Guid.Empty });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateStockInRequest.SupplierId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_quantity_fails(int qty)
    {
        var result = _sut.Validate(ValidRequest() with { Quantity = qty });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateStockInRequest.Quantity));
    }

    [Fact]
    public void Negative_unit_price_fails()
    {
        var result = _sut.Validate(ValidRequest() with { UnitPrice = -1m });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateStockInRequest.UnitPrice));
    }

    [Fact]
    public void Zero_unit_price_passes()
    {
        _sut.Validate(ValidRequest() with { UnitPrice = 0m }).IsValid.Should().BeTrue();
    }
}
