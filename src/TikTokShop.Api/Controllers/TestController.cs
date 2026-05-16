using Microsoft.AspNetCore.Mvc;
using TikTokShop.Domain.Exceptions;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    [HttpGet("throw")]
    public IActionResult ThrowNotFoundException() =>
        throw new NotFoundException("Product", Guid.NewGuid());

    [HttpGet("throw/{type}")]
    public IActionResult ThrowException(string type) => type switch
    {
        "validation" => throw new ValidationException(new Dictionary<string, string[]>
        {
            { "Name", ["Name is required"] },
            { "Price", ["Price must be greater than 0"] }
        }),
        "forbidden" => throw new ForbiddenException(),
        "conflict" => throw new ConflictException("Product", "Code", "P001"),
        "business" => throw new BusinessRuleException("Cannot sell more than available stock."),
        "server" => throw new InvalidOperationException("Simulated server error"),
        _ => throw new NotFoundException("Resource", type)
    };
}
