namespace Stateful
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// State that contains a single value
    /// </summary>
    /// <typeparam name="T">Data type of state</typeparam>
    public interface IObjectState<T> : IState
    {
        /// <summary>
        /// Attempt to retrieve value
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="ConditionalValue{T}"/> with state value, or <see cref="ConditionalValue{T}.HasValue"/> is False</returns>
        Task<ConditionalValue<T>> TryGetAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sets this state object to <paramref name="value"/>
        /// </summary>
        /// <param name="value"><typeparamref name="T"/> to set state to</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><typeparamref name="T"/> value that was set</returns>
        Task<T> SetAsync(T value, CancellationToken cancellationToken = default(CancellationToken));
    }
}