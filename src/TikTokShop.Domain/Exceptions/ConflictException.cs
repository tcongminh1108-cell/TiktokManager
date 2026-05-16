namespace TikTokShop.Domain.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409) { }

    public ConflictException(string entityName, string field, object value)
        : base($"{entityName} with {field} '{value}' already exists.", 409) { }
}
