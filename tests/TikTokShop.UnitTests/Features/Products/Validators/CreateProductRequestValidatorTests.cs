using FluentAssertions;
using TikTokShop.Application.Features.Products.Dtos;
using TikTokShop.Application.Features.Products.Validators;

namespace TikTokShop.UnitTests.Features.Products.Validators;

public class CreateProductRequestValidatorTests
{
    private readonly CreateProductRequestValidator _sut = new();

    private static CreateProductRequest ValidRequest() => new(
        Code: "PROD-001",
        Name: "Test Product",
        Description: null,
        SellingPrice: 100m,
        Unit: "pcs",
        ImageUrl: null);

    [Fact]
    public void Valid_request_passes()
    {
        _sut.Validate(ValidRequest()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_code_fails()
    {
        var result = _sut.Validate(ValidRequest() with { Code = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.Code));
    }

    [Fact]
    public void Empty_name_fails()
    {
        var result = _sut.Validate(ValidRequest() with { Name = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.Name));
    }

    [Fact]
    public void Negative_selling_price_fails()
    {
        var result = _sut.Validate(ValidRequest() with { SellingPrice = -1m });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.SellingPrice));
    }

    [Fact]
    public void Zero_selling_price_passes()
    {
        _sut.Validate(ValidRequest() with { SellingPrice = 0m }).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_unit_fails()
    {
        var result = _sut.Validate(ValidRequest() with { Unit = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.Unit));
    }
}
