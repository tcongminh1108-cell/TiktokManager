using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using TikTokShop.Application.Features.TikTok.Connections.Dtos;
using TikTokShop.Application.Interfaces;

namespace TikTokShop.Infrastructure.ExternalServices.TikTok;

public sealed class TikTokApiClient : ITikTokApiClient, IDisposable
{
    private readonly HttpClient _http;
    private readonly TikTokSettings _settings;
    private readonly ITikTokRateLimiter _rateLimiter;
    private readonly ILogger<TikTokApiClient> _logger;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    // Polly pipeline: retry transient errors (5xx + 429) with exponential backoff,
    // then trip circuit-breaker after sustained failures.
    private readonly ResiliencePipeline _resilience;

    public TikTokApiClient(
        HttpClient http,
        IOptions<TikTokSettings> options,
        ITikTokRateLimiter rateLimiter,
        ILogger<TikTokApiClient> logger)
    {
        _http = http;
        _settings = options.Value;
        _rateLimiter = rateLimiter;
        _logger = logger;

        _resilience = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<TikTokApiException>(e => e.IsRetryable),
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(1),
                UseJitter = true
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<TikTokApiException>(e => e.IsRetryable),
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromSeconds(30)
            })
            .Build();
    }

    // ─── Auth endpoints (no shop context, no rate limiter, no signing) ──────
    // TikTok token endpoints live on auth.tiktok-shops.com and use plain GET requests.

    public async Task<TikTokTokenResponse> ExchangeCodeAsync(string code, CancellationToken ct = default)
    {
        var path = "/api/v2/token/get";
        var @params = new SortedDictionary<string, string>
        {
            ["app_key"] = _settings.AppKey,
            ["app_secret"] = _settings.AppSecret,
            ["auth_code"] = code,          // TikTok expects "auth_code", not "code"
            ["grant_type"] = "authorized_code"
        };
        return await GetFromTokenApiAsync<TikTokTokenResponse>(path, @params, ct);
    }

    public async Task<TikTokTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var path = "/api/v2/token/refresh";
        var @params = new SortedDictionary<string, string>
        {
            ["app_key"] = _settings.AppKey,
            ["app_secret"] = _settings.AppSecret,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        };
        return await GetFromTokenApiAsync<TikTokTokenResponse>(path, @params, ct);
    }

    public async Task<IReadOnlyList<TikTokShopInfo>> GetAuthorizedShopsAsync(
        string accessToken, CancellationToken ct = default)
    {
        var path = "/authorization/202309/shops";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var @params = new SortedDictionary<string, string>
        {
            ["app_key"] = _settings.AppKey,
            ["timestamp"] = timestamp
        };
        @params["sign"] = ComputeSign(path, @params);

        var url = BuildUrl(_settings.ApiBaseUrl, path, @params);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("x-tts-access-token", accessToken);

        using var response = await _http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        _logger.LogDebug("GetAuthorizedShops response: {Body}", body);

        var doc = JsonNode.Parse(body);
        var shops = doc?["data"]?["shops"]?.AsArray();
        if (shops is null) return [];

        return shops
            .Where(s => s is not null)
            .Select(s => new TikTokShopInfo(
                ShopId: s!["id"]?.GetValue<string>() ?? string.Empty,
                ShopName: s["name"]?.GetValue<string>() ?? string.Empty,
                Cipher: s["cipher"]?.GetValue<string>() ?? string.Empty,
                Region: s["region"]?.GetValue<string>() ?? "GLOBAL"))
            .ToList();
    }

    // ─── Shop-scoped: Orders ─────────────────────────────────────────────────

    public async Task<TikTokOrderListResponse> GetOrdersAsync(
        TikTokApiContext ctx, TikTokOrderQueryParams queryParams, CancellationToken ct = default)
    {
        await _rateLimiter.AcquireAsync(ctx.ShopCipher, ct);

        return await _resilience.ExecuteAsync(async innerCt =>
        {
            var path = "/order/202309/orders/search";
            var (url, baseParams) = BuildShopUrl(ctx, path);

            // Body carries pagination + filter
            var body = new Dictionary<string, object> { ["page_size"] = queryParams.PageSize };
            if (queryParams.NextPageToken is not null)
                body["page_token"] = queryParams.NextPageToken;
            if (queryParams.UpdateTimeGe.HasValue)
                body["filter"] = new Dictionary<string, object> { ["update_time_ge"] = queryParams.UpdateTimeGe.Value };

            using var req = BuildSignedPostRequest(ctx, path, url, baseParams, body);
            using var resp = await _http.SendAsync(req, innerCt);
            var raw = await resp.Content.ReadAsStringAsync(innerCt);

            EnsureSuccess(raw, path);

            var doc = JsonNode.Parse(raw);
            var dataNode = doc?["data"];
            var ordersArray = dataNode?["order_list"]?.AsArray();
            var nextToken = dataNode?["next_page_token"]?.GetValue<string>();

            var orders = ordersArray?
                .Where(o => o is not null)
                .Select(o => new TikTokOrderSummary(
                    OrderId: o!["order_id"]?.GetValue<string>() ?? string.Empty,
                    StatusCode: o["order_status"]?.GetValue<int>() ?? 0,
                    UpdateTime: o["update_time"]?.GetValue<long>() ?? 0))
                .ToList() ?? [];

            return new TikTokOrderListResponse(orders, nextToken);
        }, ct);
    }

    public async Task<string?> GetOrderDetailRawAsync(
        TikTokApiContext ctx, string orderId, CancellationToken ct = default)
    {
        await _rateLimiter.AcquireAsync(ctx.ShopCipher, ct);

        return await _resilience.ExecuteAsync(async innerCt =>
        {
            // TikTok uses GET /order/202309/orders with ids query param, NOT a separate /detail path.
            var path = "/order/202309/orders";
            var (_, baseParams) = BuildShopUrl(ctx, path);
            baseParams["ids"] = $"[\"{orderId}\"]";
            baseParams["sign"] = ComputeSign(path, baseParams);

            var fullUrl = BuildUrl(ctx.BaseApiUrl, path, baseParams);
            using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            req.Headers.Add("x-tts-access-token", ctx.AccessToken);

            using var resp = await _http.SendAsync(req, innerCt);
            var raw = await resp.Content.ReadAsStringAsync(innerCt);
            EnsureSuccess(raw, path);
            return raw;
        }, ct);
    }

    public async Task<string?> GetReturnDetailRawAsync(
        TikTokApiContext ctx, string returnId, CancellationToken ct = default)
    {
        await _rateLimiter.AcquireAsync(ctx.ShopCipher, ct);

        return await _resilience.ExecuteAsync(async innerCt =>
        {
            // TikTok return APIs live under return_refund/202309, not return/202309.
            var path = "/return_refund/202309/returns";
            var (_, baseParams) = BuildShopUrl(ctx, path);
            baseParams["return_order_id"] = returnId;
            baseParams["sign"] = ComputeSign(path, baseParams);

            var fullUrl = BuildUrl(ctx.BaseApiUrl, path, baseParams);
            using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            req.Headers.Add("x-tts-access-token", ctx.AccessToken);

            using var resp = await _http.SendAsync(req, innerCt);
            var raw = await resp.Content.ReadAsStringAsync(innerCt);
            EnsureSuccess(raw, path);
            return raw;
        }, ct);
    }

    // ─── Shop-scoped: Products ────────────────────────────────────────────────

    public async Task<TikTokProductListResponse> GetTikTokProductsAsync(
        TikTokApiContext ctx, string? nextPageToken = null, string? search = null,
        int pageSize = 20, CancellationToken ct = default)
    {
        await _rateLimiter.AcquireAsync(ctx.ShopCipher, ct);

        return await _resilience.ExecuteAsync(async innerCt =>
        {
            var path = "/product/202309/products";
            var (url, baseParams) = BuildShopUrl(ctx, path);

            baseParams["page_size"] = pageSize.ToString();
            if (nextPageToken is not null) baseParams["page_token"] = nextPageToken;
            if (search is not null) baseParams["search_keyword"] = search;
            baseParams["sign"] = ComputeSign(path, baseParams);

            var fullUrl = BuildUrl(ctx.BaseApiUrl, path, baseParams);
            using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            req.Headers.Add("x-tts-access-token", ctx.AccessToken);

            using var resp = await _http.SendAsync(req, innerCt);
            var raw = await resp.Content.ReadAsStringAsync(innerCt);
            EnsureSuccess(raw, path);

            var doc = JsonNode.Parse(raw);
            var dataNode = doc?["data"];
            var productsArray = dataNode?["products"]?.AsArray();
            var npt = dataNode?["next_page_token"]?.GetValue<string>();

            var products = new List<TikTokSkuInfo>();
            if (productsArray is not null)
            {
                foreach (var p in productsArray)
                {
                    if (p is null) continue;
                    var productId = p["id"]?.GetValue<string>() ?? string.Empty;
                    var productName = p["title"]?.GetValue<string>() ?? string.Empty;
                    var skus = p["skus"]?.AsArray();
                    if (skus is not null)
                    {
                        foreach (var sku in skus)
                        {
                            if (sku is null) continue;
                            products.Add(new TikTokSkuInfo(
                                ProductId: productId,
                                ProductName: productName,
                                SkuId: sku["id"]?.GetValue<string>() ?? string.Empty,
                                SkuName: sku["seller_sku"]?.GetValue<string>() ?? string.Empty));
                        }
                    }
                }
            }

            return new TikTokProductListResponse(products, npt);
        }, ct);
    }

    // ─── Phase 7.7 — Push Inventory ───────────────────────────────────────────

    public async Task UpdateInventoryAsync(
        TikTokApiContext ctx, string tikTokProductId, string tikTokSkuId,
        int quantity, string? warehouseId = null, CancellationToken ct = default)
    {
        await _rateLimiter.AcquireAsync(ctx.ShopCipher, ct);

        await _resilience.ExecuteAsync(async innerCt =>
        {
            // Path includes productId; body uses skus[].inventory[].warehouse_id (not warehouse_type).
            var path = $"/product/202309/products/{tikTokProductId}/inventory/update";
            var (url, baseParams) = BuildShopUrl(ctx, path);

            object inventoryEntry = warehouseId is not null
                ? new { quantity, warehouse_id = warehouseId }
                : (object)new { quantity };

            var body = new
            {
                skus = new[] { new { id = tikTokSkuId, inventory = new[] { inventoryEntry } } }
            };

            using var req = BuildSignedPostRequest(ctx, path, url, baseParams, body);
            using var resp = await _http.SendAsync(req, innerCt);
            var raw = await resp.Content.ReadAsStringAsync(innerCt);
            EnsureSuccess(raw, path);
        }, ct);
    }

    // ─── Phase 7.8 — Finance API ─────────────────────────────────────────────

    public async Task<string?> GetFinanceStatementsRawAsync(
        TikTokApiContext ctx, long fromTimestamp, long toTimestamp,
        string? pageToken = null, int pageSize = 20, CancellationToken ct = default)
    {
        await _rateLimiter.AcquireAsync(ctx.ShopCipher, ct);

        return await _resilience.ExecuteAsync(async innerCt =>
        {
            var path = "/finance/202309/statements";
            var (_, baseParams) = BuildShopUrl(ctx, path);
            baseParams["from_date"] = fromTimestamp.ToString();
            baseParams["to_date"] = toTimestamp.ToString();
            baseParams["page_size"] = pageSize.ToString();
            if (pageToken is not null) baseParams["page_token"] = pageToken;
            baseParams["sign"] = ComputeSign(path, baseParams);

            var fullUrl = BuildUrl(ctx.BaseApiUrl, path, baseParams);
            using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            req.Headers.Add("x-tts-access-token", ctx.AccessToken);

            using var resp = await _http.SendAsync(req, innerCt);
            var raw = await resp.Content.ReadAsStringAsync(innerCt);
            EnsureSuccess(raw, path);
            return raw;
        }, ct);
    }

    public async Task<string?> GetFinanceStatementOrdersRawAsync(
        TikTokApiContext ctx, string statementId, string? pageToken = null, CancellationToken ct = default)
    {
        await _rateLimiter.AcquireAsync(ctx.ShopCipher, ct);

        return await _resilience.ExecuteAsync(async innerCt =>
        {
            // Correct path: /finance/202309/statements/{statementId}/statement_transactions
            var path = $"/finance/202309/statements/{statementId}/statement_transactions";
            var (_, baseParams) = BuildShopUrl(ctx, path);
            baseParams["page_size"] = "50";
            if (pageToken is not null) baseParams["page_token"] = pageToken;
            baseParams["sign"] = ComputeSign(path, baseParams);

            var fullUrl = BuildUrl(ctx.BaseApiUrl, path, baseParams);
            using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            req.Headers.Add("x-tts-access-token", ctx.AccessToken);

            using var resp = await _http.SendAsync(req, innerCt);
            var raw = await resp.Content.ReadAsStringAsync(innerCt);
            EnsureSuccess(raw, path);
            return raw;
        }, ct);
    }

    // ─── Webhook management ───────────────────────────────────────────────────

    public async Task<string?> GetShopWebhooksAsync(TikTokApiContext ctx, CancellationToken ct = default)
    {
        await _rateLimiter.AcquireAsync(ctx.ShopCipher, ct);

        return await _resilience.ExecuteAsync(async innerCt =>
        {
            var path = "/event/202309/webhooks";
            var (_, baseParams) = BuildShopUrl(ctx, path);
            baseParams["sign"] = ComputeSign(path, baseParams);

            var fullUrl = BuildUrl(ctx.BaseApiUrl, path, baseParams);
            using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            req.Headers.Add("x-tts-access-token", ctx.AccessToken);

            using var resp = await _http.SendAsync(req, innerCt);
            var raw = await resp.Content.ReadAsStringAsync(innerCt);
            EnsureSuccess(raw, path);
            return raw;
        }, ct);
    }

    public async Task RegisterWebhookAsync(
        TikTokApiContext ctx, string eventType, string callbackUrl, CancellationToken ct = default)
    {
        await _rateLimiter.AcquireAsync(ctx.ShopCipher, ct);

        await _resilience.ExecuteAsync(async innerCt =>
        {
            var path = "/event/202309/webhooks";
            var (url, baseParams) = BuildShopUrl(ctx, path);
            var body = new { address = callbackUrl, event_type = eventType };
            using var req = BuildSignedPostRequest(ctx, path, url, baseParams, body, HttpMethod.Put);
            using var resp = await _http.SendAsync(req, innerCt);
            var raw = await resp.Content.ReadAsStringAsync(innerCt);
            EnsureSuccess(raw, path);
        }, ct);
    }

    public async Task DeleteWebhookAsync(
        TikTokApiContext ctx, string eventType, CancellationToken ct = default)
    {
        await _rateLimiter.AcquireAsync(ctx.ShopCipher, ct);

        await _resilience.ExecuteAsync(async innerCt =>
        {
            var path = "/event/202309/webhooks";
            var (url, baseParams) = BuildShopUrl(ctx, path);
            var body = new { event_type = eventType };
            using var req = BuildSignedPostRequest(ctx, path, url, baseParams, body, HttpMethod.Delete);
            using var resp = await _http.SendAsync(req, innerCt);
            var raw = await resp.Content.ReadAsStringAsync(innerCt);
            EnsureSuccess(raw, path);
        }, ct);
    }

    // ─── Phase 7.9 — Video API ────────────────────────────────────────────────

    public async Task<string?> GetVideosRawAsync(
        TikTokApiContext ctx, string? pageToken = null, int pageSize = 20, CancellationToken ct = default)
    {
        await _rateLimiter.AcquireAsync(ctx.ShopCipher, ct);

        return await _resilience.ExecuteAsync(async innerCt =>
        {
            var path = "/product/202309/videos";
            var (_, baseParams) = BuildShopUrl(ctx, path);
            baseParams["page_size"] = pageSize.ToString();
            if (pageToken is not null) baseParams["page_token"] = pageToken;
            baseParams["sign"] = ComputeSign(path, baseParams);

            var fullUrl = BuildUrl(ctx.BaseApiUrl, path, baseParams);
            using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            req.Headers.Add("x-tts-access-token", ctx.AccessToken);

            using var resp = await _http.SendAsync(req, innerCt);
            var raw = await resp.Content.ReadAsStringAsync(innerCt);
            EnsureSuccess(raw, path);
            return raw;
        }, ct);
    }

    // ─── Signing & request helpers ────────────────────────────────────────────

    private (string url, SortedDictionary<string, string> @params) BuildShopUrl(
        TikTokApiContext ctx, string path)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var @params = new SortedDictionary<string, string>
        {
            ["app_key"] = _settings.AppKey,
            ["shop_cipher"] = ctx.ShopCipher,
            ["timestamp"] = timestamp
        };
        // sign added by caller after all params are known
        var url = BuildUrl(ctx.BaseApiUrl, path, @params);
        return (url, @params);
    }

    private HttpRequestMessage BuildSignedPostRequest(
        TikTokApiContext ctx, string path,
        string baseUrl, SortedDictionary<string, string> queryParams, object bodyObj,
        HttpMethod? method = null)
    {
        // Serialize body once so the same string is used for both signing and sending.
        var bodyJson = JsonSerializer.Serialize(bodyObj, _json);
        queryParams["sign"] = ComputeSign(path, queryParams, bodyJson);
        var url = BuildUrl(ctx.BaseApiUrl, path, queryParams);
        var req = new HttpRequestMessage(method ?? HttpMethod.Post, url);
        req.Headers.Add("x-tts-access-token", ctx.AccessToken);
        req.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
        return req;
    }

    // TikTok signature: HMAC-SHA256(secret, secret + path + sorted_params_KV + body_string + secret)
    private string ComputeSign(string path, SortedDictionary<string, string> sortedParams, string bodyString = "")
    {
        var sb = new StringBuilder();
        sb.Append(_settings.AppSecret);
        sb.Append(path);
        foreach (var kv in sortedParams)
        {
            if (kv.Key is "sign" or "access_token") continue;
            sb.Append(kv.Key);
            sb.Append(kv.Value);
        }
        if (!string.IsNullOrEmpty(bodyString))
            sb.Append(bodyString);
        sb.Append(_settings.AppSecret);

        var hash = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(_settings.AppSecret),
            Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string BuildUrl(string baseUrl, string path, SortedDictionary<string, string> @params)
    {
        var query = string.Join("&", @params.Select(kv =>
            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
        return $"{baseUrl}{path}?{query}";
    }

    // Token exchange/refresh: plain GET to auth.tiktok-shops.com, no signing required.
    private async Task<T> GetFromTokenApiAsync<T>(string path, SortedDictionary<string, string> @params, CancellationToken ct)
    {
        var url = BuildUrl(_settings.TokenBaseUrl, path, @params);
        using var resp = await _http.GetAsync(url, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        _logger.LogDebug("GET {Path} → {Status}: {Body}", path, (int)resp.StatusCode, body);
        EnsureSuccess(body, path);

        var doc = JsonNode.Parse(body)!;
        return JsonSerializer.Deserialize<T>(doc["data"]!.ToJsonString(), _json)
               ?? throw new InvalidOperationException("Failed to deserialize TikTok token response.");
    }

    private void EnsureSuccess(string body, string path)
    {
        using var doc = JsonDocument.Parse(body);
        var code = doc.RootElement.TryGetProperty("code", out var c) ? c.GetInt32() : -1;
        if (code == 0) return;

        var msg = doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : "Unknown error";
        var isRetryable = code is 429 or >= 500;
        _logger.LogWarning("TikTok API {Path} error {Code}: {Msg}", path, code, msg);
        throw new TikTokApiException(code, msg ?? "TikTok error", isRetryable);
    }

    public void Dispose() => _http.Dispose();
}

public sealed class TikTokApiException(int code, string message, bool isRetryable)
    : Exception(message)
{
    public int Code { get; } = code;
    public bool IsRetryable { get; } = isRetryable;
}
