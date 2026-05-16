using FluentAssertions;
using NSubstitute;
using TikTokShop.Application.Features.Auth;
using TikTokShop.Application.Features.Auth.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;
using TikTokShop.Domain.Exceptions;
using TikTokShop.Domain.Interfaces;
using TikTokShop.UnitTests.Helpers;

namespace TikTokShop.UnitTests.Features.Auth;

public class AuthServiceTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly AuthService _sut;

    private static readonly Guid TenantId = Guid.NewGuid();

    private readonly Tenant _activeTenant = new()
    {
        Id = TenantId,
        Name = "Test Shop",
        Code = "test-shop",
        Status = TenantStatus.Active
    };

    private readonly User _activeUser;

    public AuthServiceTests()
    {
        _activeUser = new User
        {
            Id = Guid.NewGuid(),
            TenantId = TenantId,
            Email = "admin@test.com",
            PasswordHash = "hashed-password",
            FullName = "Admin User",
            Role = UserRole.Admin,
            IsActive = true,
            IsDeleted = false
        };

        _sut = new AuthService(_db, _jwt, _hasher, _currentUser);
    }

    // ─── LoginAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange — build mock sets first, then assign (avoids nested Returns() issue in NSubstitute)
        var tenantsData = new List<Tenant> { _activeTenant };
        var usersData = new List<User> { _activeUser };
        var tokensData = new List<RefreshToken>();

        var tenantsMock = DbSetMock.Of(tenantsData);
        var usersMock = DbSetMock.Of(usersData);
        var tokensMock = DbSetMock.Of(tokensData);

        _db.Tenants.Returns(tenantsMock);
        _db.Users.Returns(usersMock);
        _db.RefreshTokens.Returns(tokensMock);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        _hasher.Verify("correct-password", _activeUser.PasswordHash).Returns(true);
        _jwt.GenerateAccessToken(_activeUser).Returns(("access-token", DateTimeOffset.UtcNow.AddHours(1)));
        _jwt.GenerateRefreshToken().Returns("raw-refresh-token");
        _jwt.RefreshTokenDays.Returns(7);

        // Act
        var result = await _sut.LoginAsync(
            new LoginRequest("admin@test.com", "correct-password", "test-shop"), ipAddress: null);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.User.Email.Should().Be("admin@test.com");
        result.User.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedException()
    {
        var tenantsData = new List<Tenant> { _activeTenant };
        var usersData = new List<User> { _activeUser };
        _db.Tenants.Returns(DbSetMock.Of(tenantsData));
        _db.Users.Returns(DbSetMock.Of(usersData));

        _hasher.Verify("wrong-password", _activeUser.PasswordHash).Returns(false);

        var act = () => _sut.LoginAsync(
            new LoginRequest("admin@test.com", "wrong-password", "test-shop"), ipAddress: null);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_NonExistentTenant_ThrowsUnauthorizedException()
    {
        _db.Tenants.Returns(DbSetMock.Of(new List<Tenant>()));

        var act = () => _sut.LoginAsync(
            new LoginRequest("admin@test.com", "password", "unknown-shop"), ipAddress: null);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_InactiveTenant_ThrowsUnauthorizedException()
    {
        var inactiveTenant = new Tenant
        {
            Id = TenantId,
            Code = "test-shop",
            Status = TenantStatus.Suspended
        };
        _db.Tenants.Returns(DbSetMock.Of(new List<Tenant> { inactiveTenant }));

        var act = () => _sut.LoginAsync(
            new LoginRequest("admin@test.com", "password", "test-shop"), ipAddress: null);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_DisabledUser_ThrowsUnauthorizedException()
    {
        var disabledUser = new User
        {
            TenantId = TenantId,
            Email = "admin@test.com",
            PasswordHash = "hash",
            IsActive = false,
            IsDeleted = false
        };

        _db.Tenants.Returns(DbSetMock.Of(new List<Tenant> { _activeTenant }));
        _db.Users.Returns(DbSetMock.Of(new List<User> { disabledUser }));

        var act = () => _sut.LoginAsync(
            new LoginRequest("admin@test.com", "password", "test-shop"), ipAddress: null);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    // ─── RegisterTenantAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task RegisterTenantAsync_DuplicateTenantCode_ThrowsConflictException()
    {
        _db.Tenants.Returns(DbSetMock.Of(new List<Tenant> { _activeTenant }));

        var req = new RegisterTenantRequest(
            "New Shop", "test-shop", "new@shop.com", null,
            "admin@new.com", "P@ssword1", "New Admin");

        var act = () => _sut.RegisterTenantAsync(req, ipAddress: null);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
