using FluentValidation;
using TikTokShop.Application.Features.Users.Dtos;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.Users.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Role)
            .IsInEnum()
            .Must(r => r != (UserRole)0)
            .WithMessage("Role is required.");
    }
}
