namespace Stateful
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// State of stack pattern that treats a collection as FILO (First-In-Last-Out) stack
    /// </summary>
    /// <typeparam name="T">Data type of state values</typeparam>
    public interface IStackState<T> : ICollectionState<T>
    {
        /// <summary>
        /// Try and get the next value of the stack without popping it.
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="ConditionalValue{T}"/> of next stack value, or <see cref="ConditionalValue{T}.HasValue"/> is False if the stack is empty</returns>
        Task<ConditionalValue<T>> TryPeekAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Add a new value to the top of the stack
        /// </summary>
        /// <param name="value"><typeparamref name="T"/> to add to stack</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        Task PushAsync(T value, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Try and take a value off the top of stack
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="ConditionalValue{T}"/> of <typeparamref name="T"/>, or <see cref="ConditionalValue{T}.HasValue"/> is False if the stack is empty</returns>
        Task<ConditionalValue<T>> TryPopAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}