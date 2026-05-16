using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace TikTokShop.UnitTests.Helpers;

// Enables async LINQ operators (ToListAsync, FirstOrDefaultAsync, CountAsync, etc.)
// over plain in-memory IEnumerable data inside NSubstitute mocked DbSets.
internal class TestAsyncQueryProvider<TEntity>(IQueryProvider inner) : IAsyncQueryProvider
{
    public IQueryable CreateQuery(Expression expression) =>
        new TestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
        new TestAsyncEnumerable<TElement>(expression);

    public object Execute(Expression expression) => inner.Execute(expression)!;

    public TResult Execute<TResult>(Expression expression) =>
        inner.Execute<TResult>(expression);

    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression) =>
        new TestAsyncEnumerable<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        var innerType = typeof(TResult).GetGenericArguments()[0];
        // IQueryProvider has two Execute overloads; select the generic one explicitly.
        var executeMethod = typeof(IQueryProvider)
            .GetMethods()
            .Single(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethodDefinition);
        var result = executeMethod.MakeGenericMethod(innerType).Invoke(inner, [expression]);
        return (TResult)typeof(Task)
            .GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(innerType)
            .Invoke(null, [result])!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(Expression expression) : base(expression) { }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
}

internal class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public T Current => inner.Current;
    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(inner.MoveNext());
    public ValueTask DisposeAsync() { inner.Dispose(); return ValueTask.CompletedTask; }
}
