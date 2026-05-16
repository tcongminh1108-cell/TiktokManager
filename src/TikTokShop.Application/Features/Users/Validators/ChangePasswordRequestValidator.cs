using FluentValidation;
using TikTokShop.Application.Features.Users.Dtos;

namespace TikTokShop.Application.Features.Users.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords do not match.");
    }
}
