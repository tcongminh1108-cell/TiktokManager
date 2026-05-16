using FluentAssertions;
using NSubstitute;
using TikTokShop.Application.Features.Products;
using TikTokShop.Application.Features.Products.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Exceptions;
using TikTokShop.Domain.Interfaces;
using TikTokShop.UnitTests.Helpers;

namespace TikTokShop.UnitTests.Features.Products;

public class ProductServiceTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly ProductService _sut;

    private static readonly Guid TenantId = Guid.NewGuid();

    public ProductServiceTests()
    {
        _currentUser.TenantId.Returns(TenantId);
        _currentUser.IsAuthenticated.Returns(true);
        _sut = new ProductService(_db, _currentUser);
    }

    [Fact]
    public async Task CreateProductAsync_NewCode_ReturnsDto()
    {
        // Arrange
        var productsData = new List<Product>();
        var mockSet = DbSetMock.Of(productsData);
        _db.Products.Returns(mockSet);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _sut.CreateProductAsync(
            new CreateProductRequest("PROD-001", "Test Product", null, 100m, "pcs", null));

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("PROD-001");
        result.Name.Should().Be("Test Product");
        result.SellingPrice.Should().Be(100m);
        productsData.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateProductAsync_DuplicateCode_ThrowsConflictException()
    {
        // Arrange
        var existing = new Product
        {
            TenantId = TenantId,
            Code = "PROD-001",
            Name = "Existing",
            Unit = "pcs",
            IsActive = true
        };
        _db.Products.Returns(DbSetMock.Of(new List<Product> { existing }));

        // Act & Assert
        var act = () => _sut.CreateProductAsync(
            new CreateProductRequest("PROD-001", "Another Product", null, 50m, "pcs", null));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task GetProductByIdAsync_NonExistentId_ThrowsNotFoundException()
    {
        _db.Products.Returns(DbSetMock.Of(new List<Product>()));

        var act = () => _sut.GetProductByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetProductByIdAsync_ExistingProduct_ReturnsDto()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            TenantId = TenantId,
            Code = "P-001",
            Name = "My Product",
            Unit = "kg",
            SellingPrice = 200m,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Products.Returns(DbSetMock.Of(new List<Product> { product }));

        // Act
        var result = await _sut.GetProductByIdAsync(productId);

        // Assert
        result.Id.Should().Be(productId);
        result.Code.Should().Be("P-001");
        result.SellingPrice.Should().Be(200m);
    }
}
