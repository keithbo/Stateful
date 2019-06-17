namespace Stateful
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public static class CollectionExtensions
    {
        /// <summary>
        /// Test <paramref name="collection"/> for existence of value, using an <see cref="IEqualityComparer{T}"/> for type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Data type of collection values</typeparam>
        /// <param name="collection"><see cref="ICollectionState{T}"/> to search</param>
        /// <param name="value">Data value to search for</param>
        /// <param name="equalityComparer"><see cref="IEqualityComparer{T}"/> used to compare <paramref name="value"/> against items in <paramref name="collection"/></param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>True if value exists in <paramref name="collection"/>, False otherwise</returns>
        public static async Task<bool> ContainsAsync<T>(this ICollectionState<T> collection, T value, IEqualityComparer<T> equalityComparer, CancellationToken cancellationToken = default(CancellationToken))
        {
            equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;

            return await collection.ContainsAsync(currentValue => equalityComparer.Equals(value, currentValue), cancellationToken);
        }
    }
}