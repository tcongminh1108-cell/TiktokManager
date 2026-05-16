using FluentValidation;
using TikTokShop.Application.Features.Suppliers.Dtos;

namespace TikTokShop.Application.Features.Suppliers.Validators;

public class UpdateSupplierRequestValidator : AbstractValidator<UpdateSupplierRequest>
{
    public UpdateSupplierRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(20).When(x => x.Phone is not null);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(200).When(x => x.Email is not null);
    }
}
