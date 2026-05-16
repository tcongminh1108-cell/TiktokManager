using FluentValidation;
using TikTokShop.Application.Features.Users.Dtos;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.Users.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Role)
            .IsInEnum()
            .Must(r => r != (UserRole)0)
            .WithMessage("Role is required.");
    }
}
