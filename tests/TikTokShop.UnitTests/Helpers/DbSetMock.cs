using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections;
using System.Linq.Expressions;

namespace TikTokShop.UnitTests.Helpers;

/// <summary>
/// Returns a lightweight, in-memory backed <see cref="DbSet{T}"/> suitable for unit tests.
/// <c>Add()</c> calls are immediately tracked in the backing list so callers can assert on it.
/// Async LINQ operators (ToListAsync, FirstOrDefaultAsync, etc.) work via
/// <see cref="TestAsyncQueryProvider{TEntity}"/> without hitting a real database.
/// </summary>
internal static class DbSetMock
{
    public static DbSet<T> Of<T>(List<T> data) where T : class
        => new FakeDbSet<T>(data);
}

/// <summary>
/// A real subclass of <see cref="DbSet{T}"/> backed by an in-memory <see cref="List{T}"/>.
/// Explicit interface implementations of <see cref="IQueryable{T}"/> and
/// <see cref="IAsyncEnumerable{T}"/> shadow the base-class ones so NSubstitute is not involved.
/// </summary>
internal sealed class FakeDbSet<T> : DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>
    where T : class
{
    private readonly List<T> _data;

    public FakeDbSet(List<T> data) => _data = data;

    // Only abstract member on DbSet<T> — not used in unit tests.
    public override IEntityType EntityType => throw new NotSupportedException();

    // ── IQueryable<T> (shadows the base-class explicit implementation) ──────────
    IQueryProvider IQueryable.Provider
        => new TestAsyncQueryProvider<T>(_data.AsQueryable().Provider);
    Expression IQueryable.Expression => _data.AsQueryable().Expression;
    Type IQueryable.ElementType => typeof(T);

    // ── IEnumerable<T> ─────────────────────────────────────────────────────────
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => _data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

    // ── IAsyncEnumerable<T> ────────────────────────────────────────────────────
    IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken)
        => new TestAsyncEnumerator<T>(_data.GetEnumerator());

    // ── DbSet<T> mutation helpers ──────────────────────────────────────────────
    // Override so that Add() writes to _data instead of calling into a real DbContext.
    public override EntityEntry<T> Add(T entity)
    {
        _data.Add(entity);
        return null!; // callers in service code never use the return value
    }

    public override ValueTask<EntityEntry<T>> AddAsync(
        T entity, CancellationToken cancellationToken = default)
    {
        _data.Add(entity);
        return new ValueTask<EntityEntry<T>>(result: null!);
    }
}
