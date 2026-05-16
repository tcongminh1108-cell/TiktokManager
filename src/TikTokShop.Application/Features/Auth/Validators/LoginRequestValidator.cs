using FluentValidation;
using TikTokShop.Application.Features.Auth.Dtos;

namespace TikTokShop.Application.Features.Auth.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.TenantCode).NotEmpty();
    }
}
