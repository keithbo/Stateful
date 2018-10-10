namespace Stateful
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IListState<T> : ICollectionState<T>
    {
        Task<bool> ContainsAsync(T value, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<T>> GetAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<T> GetAsync(int index, CancellationToken cancellationToken = default(CancellationToken));

        Task<T> FindAsync(Predicate<T> match, CancellationToken cancellationToken = default(CancellationToken));

        Task InsertAsync(int index, T value, CancellationToken cancellationToken = default(CancellationToken));

        Task RemoveAtAsync(int index, CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> RemoveAsync(T value, CancellationToken cancellationToken = default(CancellationToken));
    }
}