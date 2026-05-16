namespace TikTokShop.Domain.Exceptions;

public class BusinessRuleException : AppException
{
    public BusinessRuleException(string message) : base(message, 422) { }
}
