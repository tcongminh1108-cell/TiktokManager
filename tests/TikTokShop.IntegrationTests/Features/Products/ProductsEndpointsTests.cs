using TikTokShop.Application.Common.Models;

namespace TikTokShop.IntegrationTests.Features.Products;

[Collection("Integration")]
public class ProductsEndpointsTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private HttpClient _client = null!;
    private string _token = null!;

    // Each test class uses a unique tenant so data doesn't leak between classes
    private static readonly string TenantCode = $"prod-{Guid.NewGuid():N}"[..20];

    public async Task InitializeAsync()
    {
        _client = CreateClient();
        _token = await RegisterAndLoginAsync(_client, TenantCode);
        WithBearer(_client, _token);
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    // ── GET /api/products ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_Unauthenticated_Returns401()
    {
        var unauthed = CreateClient();
        var response = await unauthed.GetAsync("/api/products");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProducts_Authenticated_Returns200WithPaginatedResult()
    {
        var response = await _client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await ReadDataAsync<PaginatedResult<ProductDto>>(response);
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
    }

    // ── POST /api/products ──────────────────────────────────────────────────────

    [Fact]
    public async Task CreateProduct_ValidRequest_Returns201WithDto()
    {
        var request = new CreateProductRequest(
            Code: $"P-{Guid.NewGuid():N}"[..12],
            Name: "Test Product",
            Description: "A test product",
            SellingPrice: 99.99m,
            Unit: "pcs",
            ImageUrl: null);

        var response = await _client.PostAsJsonAsync("/api/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = await ReadDataAsync<ProductDto>(response);
        product.Code.Should().Be(request.Code);
        product.Name.Should().Be("Test Product");
        product.SellingPrice.Should().Be(99.99m);
    }

    [Fact]
    public async Task CreateProduct_DuplicateCode_Returns409()
    {
        var code = $"DUP-{Guid.NewGuid():N}"[..12];
        var request = new CreateProductRequest(code, "First", null, 10m, "pcs", null);

        await _client.PostAsJsonAsync("/api/products", request);
        var secondResponse = await _client.PostAsJsonAsync("/api/products", request);

        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateProduct_InvalidRequest_Returns400()
    {
        // Empty code should fail validation
        var request = new CreateProductRequest("", "Name", null, 10m, "pcs", null);

        var response = await _client.PostAsJsonAsync("/api/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET /api/products/{id} ──────────────────────────────────────────────────

    [Fact]
    public async Task GetProductById_ExistingId_Returns200WithDto()
    {
        var code = $"GET-{Guid.NewGuid():N}"[..12];
        var createResp = await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest(code, "FindMe", null, 50m, "kg", null));
        var created = await ReadDataAsync<ProductDto>(createResp);

        var getResp = await _client.GetAsync($"/api/products/{created.Id}");

        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var found = await ReadDataAsync<ProductDto>(getResp);
        found.Id.Should().Be(created.Id);
        found.Code.Should().Be(code);
    }

    [Fact]
    public async Task GetProductById_NonExistentId_Returns404()
    {
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
