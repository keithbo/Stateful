namespace Stateful
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IObjectState<T> : IState
    {
        Task<ConditionalValue<T>> TryGetAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<T> SetAsync(T value, CancellationToken cancellationToken = default(CancellationToken));
    }
}