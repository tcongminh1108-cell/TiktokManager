using FluentValidation;
using TikTokShop.Application.Features.Auth.Dtos;

namespace TikTokShop.Application.Features.Auth.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
