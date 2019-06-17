namespace Stateful
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Base interface all state shares
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Get key used to identify this state
        /// </summary>
        IStateKey Key { get; }

        /// <summary>
        /// Check to see if this state has an underlying persisted value.
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>True if persisted state exists, False otherwise</returns>
        Task<bool> HasStateAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Delete the underlying persisted value.
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        Task DeleteStateAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
