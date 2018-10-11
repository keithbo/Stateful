namespace Stateful
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDictionaryState<TKey, TValue> : ICollectionState<KeyValuePair<TKey, TValue>>
        where TKey : IEquatable<TKey>, IComparable<TKey>
    {
        Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken = default(CancellationToken));

        Task AddAsync(TKey key, TValue value, CancellationToken cancellationToken = default(CancellationToken));

        Task<ConditionalValue<TValue>> TryGetValueAsync(TKey key, CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> RemoveAsync(TKey key, CancellationToken cancellationToken = default(CancellationToken));
    }
}