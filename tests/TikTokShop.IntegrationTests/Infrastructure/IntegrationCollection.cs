namespace TikTokShop.IntegrationTests.Infrastructure;

/// <summary>
/// Declares the "Integration" xUnit collection so all integration test classes
/// share a single <see cref="CustomWebApplicationFactory"/> (one PostgreSQL container
/// for the entire test run).
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<CustomWebApplicationFactory> { }
