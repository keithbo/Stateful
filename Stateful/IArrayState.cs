namespace Stateful
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A <see cref="ICollectionState{T}"/> with a fixed number of contained values
    /// </summary>
    /// <typeparam name="T">Type of state value</typeparam>
    public interface IArrayState<T> : ICollectionState<T>
    {
        /// <summary>
        /// The number of values possible in this array.
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Gets the <typeparamref name="T"/> at <paramref name="index"/>
        /// </summary>
        /// <param name="index">Index of item to retrieve</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><typeparamref name="T"/> at the specified <paramref name="index"/></returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than the configured length of this state array</exception>
        Task<T> GetAsync(long index, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sets the value at <paramref name="index"/> to <paramref name="value"/>
        /// </summary>
        /// <param name="index">Long index of item to set</param>
        /// <param name="value">Value to place at <paramref name="index"/></param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than the configured length of this state array</exception>
        Task SetAsync(long index, T value, CancellationToken cancellationToken = default(CancellationToken));
    }
}