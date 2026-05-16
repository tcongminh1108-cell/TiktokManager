using FluentValidation;
using TikTokShop.Application.Features.StockIns.Dtos;

namespace TikTokShop.Application.Features.StockIns.Validators;

public class UpdateStockInRequestValidator : AbstractValidator<UpdateStockInRequest>
{
    public UpdateStockInRequestValidator()
    {
        RuleFor(x => x.TransactionDate).NotEmpty();
    }
}
