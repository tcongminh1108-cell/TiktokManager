using FluentAssertions;
using TikTokShop.Application.Features.Auth.Dtos;
using TikTokShop.Application.Features.Auth.Validators;

namespace TikTokShop.UnitTests.Features.Auth.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _sut = new();

    [Fact]
    public void Valid_request_passes()
    {
        var req = new LoginRequest("admin@shop.com", "password123", "my-shop");
        _sut.Validate(req).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "password123", "my-shop")]
    [InlineData("notanemail", "password123", "my-shop")]
    [InlineData("admin@shop.com", "", "my-shop")]
    [InlineData("admin@shop.com", "password123", "")]
    public void Invalid_request_fails(string email, string password, string tenantCode)
    {
        var req = new LoginRequest(email, password, tenantCode);
        _sut.Validate(req).IsValid.Should().BeFalse();
    }
}
