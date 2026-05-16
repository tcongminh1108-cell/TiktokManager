namespace TikTokShop.IntegrationTests.Infrastructure;

/// <summary>
/// Shared helpers for integration test classes.
/// </summary>
public abstract class IntegrationTestBase(CustomWebApplicationFactory factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    // ── Auth helpers ────────────────────────────────────────────────────────────

    protected HttpClient CreateClient() => factory.CreateApiClient();

    /// <summary>Registers a new tenant + admin user and returns the access token.</summary>
    protected async Task<string> RegisterAndLoginAsync(
        HttpClient client,
        string tenantCode,
        string adminEmail = "admin@test.com",
        string adminPassword = "P@ssw0rd123!")
    {
        var registerRequest = new RegisterTenantRequest(
            TenantName: $"Test Shop ({tenantCode})",
            TenantCode: tenantCode,
            ContactEmail: adminEmail,
            ContactPhone: null,
            AdminEmail: adminEmail,
            AdminPassword: adminPassword,
            AdminFullName: "Test Admin");

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.IsSuccessStatusCode.Should().BeTrue(
            $"tenant registration failed with {registerResponse.StatusCode}: {await registerResponse.Content.ReadAsStringAsync()}");

        return await LoginAsync(client, tenantCode, adminEmail, adminPassword);
    }

    /// <summary>Logs in and returns the raw access token string.</summary>
    protected async Task<string> LoginAsync(
        HttpClient client,
        string tenantCode,
        string email,
        string password)
    {
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, password, tenantCode));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(JsonOptions);
        body!.Data!.AccessToken.Should().NotBeNullOrEmpty();
        return body.Data.AccessToken;
    }

    /// <summary>Creates an <see cref="HttpClient"/> pre-configured with a Bearer token.</summary>
    protected static HttpClient WithBearer(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected async Task<T> ReadDataAsync<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue(body.Message);
        return body.Data!;
    }
}
