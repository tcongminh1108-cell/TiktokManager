using FluentValidation;
using TikTokShop.Application.Features.StockOuts.Dtos;

namespace TikTokShop.Application.Features.StockOuts.Validators;

public class CreateStockOutRequestValidator : AbstractValidator<CreateStockOutRequest>
{
    public CreateStockOutRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TransactionDate).NotEmpty();
        RuleFor(x => x.CustomerName).MaximumLength(200).When(x => x.CustomerName is not null);
    }
}
