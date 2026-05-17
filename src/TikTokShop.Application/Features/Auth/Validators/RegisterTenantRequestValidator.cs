using FluentValidation;
using TikTokShop.Application.Features.Auth.Dtos;

namespace TikTokShop.Application.Features.Auth.Validators;

public class RegisterTenantRequestValidator : AbstractValidator<RegisterTenantRequest>
{
    public RegisterTenantRequestValidator()
    {
        RuleFor(x => x.TenantName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ContactEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.ContactPhone)
            .MaximumLength(20)
            .When(x => x.ContactPhone is not null);

        RuleFor(x => x.AdminEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.AdminPassword)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);

        RuleFor(x => x.AdminFullName)
            .NotEmpty()
            .MaximumLength(100);
    }
}
