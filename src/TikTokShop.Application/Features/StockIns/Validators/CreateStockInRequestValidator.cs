using FluentValidation;
using TikTokShop.Application.Features.StockIns.Dtos;

namespace TikTokShop.Application.Features.StockIns.Validators;

public class CreateStockInRequestValidator : AbstractValidator<CreateStockInRequest>
{
    public CreateStockInRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TransactionDate).NotEmpty();
    }
}
