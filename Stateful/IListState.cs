namespace Stateful
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IListState<T> : ICollectionState<T>
    {
        Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<ConditionalValue<T>> TryGetAsync(long index, CancellationToken cancellationToken = default(CancellationToken));

        Task<ConditionalValue<T>> TryFindAsync(Predicate<T> match, CancellationToken cancellationToken = default(CancellationToken));

        Task InsertAsync(long index, T value, CancellationToken cancellationToken = default(CancellationToken));

        Task RemoveAtAsync(long index, CancellationToken cancellationToken = default(CancellationToken));

        Task RemoveAsync(Predicate<T> match, CancellationToken cancellationToken = default(CancellationToken));
    }
}