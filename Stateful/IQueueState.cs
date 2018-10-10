namespace Stateful
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IQueueState<T> : ICollectionState<T>
    {
        Task<ConditionalValue<T>> TryPeekAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task EnqueueAsync(T value, CancellationToken cancellationToken = default(CancellationToken));

        Task<ConditionalValue<T>> TryDequeueAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}