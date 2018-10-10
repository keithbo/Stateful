namespace Stateful
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IStackState<T> : ICollectionState<T>
    {
        Task<ConditionalValue<T>> TryPeekAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task PushAsync(T value, CancellationToken cancellationToken = default(CancellationToken));

        Task<ConditionalValue<T>> TryPopAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}