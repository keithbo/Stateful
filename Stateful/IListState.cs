namespace Stateful
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// State that contains a sequential collection of values, accessible by indexed value.
    /// </summary>
    /// <typeparam name="T">Data type of the state values</typeparam>
    public interface IListState<T> : ICollectionState<T>
    {
        /// <summary>
        /// Get all values from this state
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="ConditionalValue{T}"/> of <see cref="IList{T}"/> that has the entire list or <see cref="ConditionalValue{T}.HasValue"/> is False</returns>
        Task<ConditionalValue<IList<T>>> TryGetAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get a state value for a specific <paramref name="index"/>
        /// </summary>
        /// <param name="index">Index of the value to retrieve</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="ConditionalValue{T}"/> with the retrieved value, or <see cref="ConditionalValue{T}.HasValue"/> is False</returns>
        Task<ConditionalValue<T>> TryGetAsync(long index, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Search this state list using <paramref name="match"/> predicate and return the value if found.
        /// </summary>
        /// <param name="match"><see cref="Predicate{T}"/></param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="ConditionalValue{T}"/> with matched value, or <see cref="ConditionalValue{T}.HasValue"/> is False</returns>
        Task<ConditionalValue<T>> TryFindAsync(Predicate<T> match, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Add a value to the end of this state list
        /// </summary>
        /// <param name="value"><typeparamref name="T"/> value to add to state</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        Task AddAsync(T value, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Add multiple values to the end of this state list
        /// </summary>
        /// <param name="values"><see cref="IEnumerable{T}"/> of <typeparamref name="T"/> values to add to state</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        Task AddRangeAsync(IEnumerable<T> values, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Insert <paramref name="value"/> at <paramref name="index"/> of this state list
        /// </summary>
        /// <param name="index">Index to insert <paramref name="value"/> at</param>
        /// <param name="value"><typeparamref name="T"/> value to add to state</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> if index is less than 0 or greater than the length of the list</exception>
        Task InsertAsync(long index, T value, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Insert <paramref name="values"/> at <paramref name="index"/> of this state list
        /// </summary>
        /// <param name="index">Index to insert <paramref name="values"/> at</param>
        /// <param name="values"><see cref="IEnumerable{T}"/> of <typeparamref name="T"/> to add to state</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> if index is less than 0 or greater than the length of the list</exception>
        Task InsertRangeAsync(long index, IEnumerable<T> values, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Remove value at <paramref name="index"/>
        /// </summary>
        /// <param name="index">Index to remove value at</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> if index is less than 0 or greater than the length of the list</exception>
        Task RemoveAtAsync(long index, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Remove the first value that <paramref name="match"/> returns true for
        /// </summary>
        /// <param name="match"><see cref="Predicate{T}"/> to identify value to remove</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        Task RemoveAsync(Predicate<T> match, CancellationToken cancellationToken = default(CancellationToken));
    }
}