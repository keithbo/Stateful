namespace Stateful
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public static class CollectionExtensions
    {
        public static async Task<bool> ContainsAsync<T>(this ICollectionState<T> collection, T value, IEqualityComparer<T> equalityComparer, CancellationToken cancellationToken = default(CancellationToken))
        {
            equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;

            return await collection.ContainsAsync(currentValue => equalityComparer.Equals(value, currentValue), cancellationToken);
        }
    }
}