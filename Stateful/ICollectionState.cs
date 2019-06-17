namespace Stateful
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// State that contains an enumerable set of values
    /// </summary>
    /// <typeparam name="T">Type of the state value</typeparam>
    public interface ICollectionState<T> : IEnumerableState<T>
    {
        /// <summary>
        /// Get the number of values in this collection
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>long number of values in this collection</returns>
        Task<long> CountAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Test this collection has any value matching the provided <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate"><see cref="Predicate{T}"/> to test each value for success</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>True if the predicate succeeds, False otherwise</returns>
        Task<bool> ContainsAsync(Predicate<T> predicate, CancellationToken cancellationToken = default(CancellationToken));
    }
}