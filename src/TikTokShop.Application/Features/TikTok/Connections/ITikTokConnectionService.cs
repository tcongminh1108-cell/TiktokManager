using TikTokShop.Application.Features.TikTok.Connections.Dtos;

namespace TikTokShop.Application.Features.TikTok.Connections;

public interface ITikTokConnectionService
{
    Task<TikTokAuthUrlResponse> GetAuthUrlAsync(string? redirectAfter, CancellationToken ct = default);
    Task<int> HandleCallbackAsync(string code, string state, CancellationToken ct = default);
    Task<IReadOnlyList<TikTokConnectionDto>> GetConnectionsAsync(CancellationToken ct = default);
    Task<TikTokConnectionDto> GetConnectionByIdAsync(Guid id, CancellationToken ct = default);
    Task DeleteConnectionAsync(Guid id, CancellationToken ct = default);
    Task RefreshTokenAsync(Guid id, CancellationToken ct = default);
}
