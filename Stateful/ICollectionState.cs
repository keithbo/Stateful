namespace Stateful
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICollectionState<T> : IEnumerableState<T>
    {
        Task<long> CountAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> ContainsAsync(Predicate<T> predicate, CancellationToken cancellationToken = default(CancellationToken));
    }
}