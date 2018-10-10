namespace Stateful
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IBagState<T> : ICollectionState<T>
    {
        Task<ConditionalValue<T>> TryPeekAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<ConditionalValue<T>> TryTakeAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}