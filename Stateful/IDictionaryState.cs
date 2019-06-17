namespace Stateful
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// State that contains Key/Value pairs. State is a collection of <see cref="KeyValuePair{TKey,TValue}"/>
    /// </summary>
    /// <typeparam name="TKey">Dictionary key data type. Must be <see cref="IEquatable{T}"/> and <see cref="IComparable{T}"/></typeparam>
    /// <typeparam name="TValue">Dictionary paired value data type</typeparam>
    public interface IDictionaryState<TKey, TValue> : ICollectionState<KeyValuePair<TKey, TValue>>
        where TKey : IEquatable<TKey>, IComparable<TKey>
    {
        /// <summary>
        /// Test this dictionary state if it contains the provided <paramref name="key"/>
        /// </summary>
        /// <param name="key"><typeparamref name="TKey"/> to test existence of</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>True if key exists in the dictionary, False otherwise</returns>
        Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Add a key/value pair to this dictionary state
        /// </summary>
        /// <param name="key"><typeparamref name="TKey"/> key value</param>
        /// <param name="value"><typeparamref name="TValue"/> paired value</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        Task AddAsync(TKey key, TValue value, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Attempt to get a state dictionary value by its <paramref name="key"/>.
        /// </summary>
        /// <param name="key"><typeparamref name="TKey"/> key to try and look up value for</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="ConditionalValue{T}"/> of the looked up value. Result will contain the value looked for, or <see cref="ConditionalValue{T}.HasValue"/> will be false</returns>
        Task<ConditionalValue<TValue>> TryGetValueAsync(TKey key, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Attempt to remove <paramref name="key"/> from this dictionary state.
        /// </summary>
        /// <param name="key"><typeparamref name="TKey"/> key to try and remove</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>True if the value was successfully removed, False otherwise</returns>
        Task<bool> RemoveAsync(TKey key, CancellationToken cancellationToken = default(CancellationToken));
    }
}