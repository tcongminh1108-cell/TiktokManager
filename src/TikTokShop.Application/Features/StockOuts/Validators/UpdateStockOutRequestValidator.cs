using FluentValidation;
using TikTokShop.Application.Features.StockOuts.Dtos;

namespace TikTokShop.Application.Features.StockOuts.Validators;

public class UpdateStockOutRequestValidator : AbstractValidator<UpdateStockOutRequest>
{
    public UpdateStockOutRequestValidator()
    {
        RuleFor(x => x.TransactionDate).NotEmpty();
        RuleFor(x => x.CustomerName).MaximumLength(200).When(x => x.CustomerName is not null);
    }
}
