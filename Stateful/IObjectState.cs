namespace Stateful
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IObjectState<T> : IState
    {
        Task<T> GetAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<TResult> GetAsync<TResult>(Func<T, TResult> valueAccessor, CancellationToken cancellationToken = default(CancellationToken));

        Task<T> SetAsync(T value, CancellationToken cancellationToken = default(CancellationToken));
    }
}