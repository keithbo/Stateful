namespace Stateful
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Enumerate a state values
    /// </summary>
    /// <typeparam name="T">Data type of state</typeparam>
    public interface IAsyncEnumerator<out T> : IDisposable
    {
        /// <summary>
        /// Current state value of this enumerator
        /// </summary>
        T Current { get; }

        /// <summary>
        /// Advance this enumerator to the next state value
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>True if enumerator could be advanced, False otherwise</returns>
        Task<bool> MoveNextAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Reset this enumerator to enumerate from the beginning
        /// </summary>
        void Reset();
    }
}