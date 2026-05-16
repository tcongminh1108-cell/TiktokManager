using FluentAssertions;
using TikTokShop.Application.Features.Auth.Dtos;
using TikTokShop.Application.Features.Auth.Validators;

namespace TikTokShop.UnitTests.Features.Auth.Validators;

public class RegisterTenantRequestValidatorTests
{
    private readonly RegisterTenantRequestValidator _sut = new();

    private static RegisterTenantRequest ValidRequest() => new(
        TenantName: "My Shop",
        TenantCode: "my-shop",
        ContactEmail: "contact@shop.com",
        ContactPhone: null,
        AdminEmail: "admin@shop.com",
        AdminPassword: "P@ssword1",
        AdminFullName: "Admin User");

    [Fact]
    public void Valid_request_passes()
    {
        _sut.Validate(ValidRequest()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("MY_SHOP")]      // uppercase not allowed
    [InlineData("MY SHOP")]      // space not allowed
    [InlineData("MY.SHOP")]      // dot not allowed
    public void TenantCode_with_invalid_characters_fails(string code)
    {
        var req = ValidRequest() with { TenantCode = code };
        var result = _sut.Validate(req);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterTenantRequest.TenantCode));
    }

    [Fact]
    public void AdminPassword_shorter_than_8_chars_fails()
    {
        var req = ValidRequest() with { AdminPassword = "short" };
        var result = _sut.Validate(req);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterTenantRequest.AdminPassword));
    }

    [Fact]
    public void Invalid_admin_email_fails()
    {
        var req = ValidRequest() with { AdminEmail = "notanemail" };
        var result = _sut.Validate(req);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterTenantRequest.AdminEmail));
    }

    [Fact]
    public void Empty_tenant_name_fails()
    {
        var req = ValidRequest() with { TenantName = "" };
        _sut.Validate(req).IsValid.Should().BeFalse();
    }
}
