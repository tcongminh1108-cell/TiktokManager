using FluentValidation;
using TikTokShop.Application.Features.Products.Dtos;

namespace TikTokShop.Application.Features.Products.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ImageUrl).MaximumLength(500).When(x => x.ImageUrl is not null);
    }
}
