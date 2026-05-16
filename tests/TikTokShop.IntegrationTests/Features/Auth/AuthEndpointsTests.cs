using TikTokShop.Domain.Enums;

namespace TikTokShop.IntegrationTests.Features.Auth;

[Collection("Integration")]
public class AuthEndpointsTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    private const string TenantCode = "auth-test-shop";
    private const string AdminEmail = "auth-admin@test.com";
    private const string AdminPassword = "P@ssw0rd123!";

    // ── POST /api/auth/register ─────────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidRequest_Returns200WithAuthResponse()
    {
        var client = CreateClient();
        var code = $"reg-{Guid.NewGuid():N}"[..20];

        var request = new RegisterTenantRequest(
            TenantName: "New Shop",
            TenantCode: code,
            ContactEmail: "contact@newshop.com",
            ContactPhone: null,
            AdminEmail: "admin@newshop.com",
            AdminPassword: "P@ssw0rd123!",
            AdminFullName: "New Admin");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await ReadDataAsync<AuthResponse>(response);
        data.AccessToken.Should().NotBeNullOrEmpty();
        data.User.Email.Should().Be("admin@newshop.com");
        data.User.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task Register_DuplicateTenantCode_Returns409()
    {
        var client = CreateClient();
        var code = $"dup-{Guid.NewGuid():N}"[..20];

        var request = new RegisterTenantRequest(
            "Shop A", code, "a@shop.com", null, "admin@a.com", "P@ssw0rd123!", "Admin A");

        await client.PostAsJsonAsync("/api/auth/register", request);
        var secondResponse = await client.PostAsJsonAsync("/api/auth/register", request);

        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ── POST /api/auth/login ────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithTokens()
    {
        var client = CreateClient();
        await RegisterAndLoginAsync(client, TenantCode, AdminEmail, AdminPassword);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(AdminEmail, AdminPassword, TenantCode));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await ReadDataAsync<AuthResponse>(loginResponse);
        data.AccessToken.Should().NotBeNullOrEmpty();
        data.RefreshToken.Should().NotBeNullOrEmpty();
        data.User.Email.Should().Be(AdminEmail);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var client = CreateClient();
        var code = $"wp-{Guid.NewGuid():N}"[..20];
        await RegisterAndLoginAsync(client, code);

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("admin@test.com", "WrongPassword!", code));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_UnknownTenant_Returns401()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("someone@test.com", "pass", "tenant-that-does-not-exist"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── POST /api/auth/refresh ──────────────────────────────────────────────────

    [Fact]
    public async Task RefreshToken_ValidToken_Returns200WithNewAccessToken()
    {
        var client = CreateClient();
        var code = $"rt-{Guid.NewGuid():N}"[..20];
        await RegisterAndLoginAsync(client, code);

        // Login first to get refresh token
        var loginResp = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("admin@test.com", "P@ssw0rd123!", code));
        var auth = await ReadDataAsync<AuthResponse>(loginResp);

        var response = await client.PostAsJsonAsync("/api/auth/refresh",
            new { RefreshToken = auth.RefreshToken });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var newAuth = await ReadDataAsync<AuthResponse>(response);
        newAuth.AccessToken.Should().NotBeNullOrEmpty();
    }
}
