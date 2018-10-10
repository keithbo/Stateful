namespace Stateful
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICollectionState<T> : IEnumerableState<T>
    {
        Task<int> CountAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task AddAsync(T value, CancellationToken cancellationToken = default(CancellationToken));

        Task AddRangeAsync(IEnumerable<T> values, CancellationToken cancellationToken = default(CancellationToken));
    }
}