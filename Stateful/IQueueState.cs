namespace Stateful
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// State of queue pattern that treats a collection as a FIFO (First-In-First-Out) queue
    /// </summary>
    /// <typeparam name="T">Data type of state values</typeparam>
    public interface IQueueState<T> : ICollectionState<T>
    {
        /// <summary>
        /// Try and get the next value of the queue without dequeueing it.
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="ConditionalValue{T}"/> of next queued value, or <see cref="ConditionalValue{T}.HasValue"/> is False if the queue is empty</returns>
        Task<ConditionalValue<T>> TryPeekAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Add a new value to the end of the queue
        /// </summary>
        /// <param name="value"><typeparamref name="T"/> to add to queue</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        Task EnqueueAsync(T value, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Try and take the next value off the queue
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="ConditionalValue{T}"/> of <typeparamref name="T"/>, or <see cref="ConditionalValue{T}.HasValue"/> is False if the queue is empty</returns>
        Task<ConditionalValue<T>> TryDequeueAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}