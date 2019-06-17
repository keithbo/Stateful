namespace Stateful
{
    using System;

    public static class UnitExtensions
    {
        /// <summary>
        /// Get an <see cref="IObjectState{T}"/> instance
        /// </summary>
        /// <typeparam name="TValue">Data type of state</typeparam>
        /// <param name="unit"><see cref="IUnit"/> to retrieve state for</param>
        /// <param name="key"><see cref="IStateKey"/> to get state object for</param>
        /// <returns><see cref="IObjectState{T}"/> of <typeparamref name="TValue"/></returns>
        public static IObjectState<TValue> GetObject<TValue>(this IUnit unit, IStateKey key)
        {
            return unit.Get<IObjectState<TValue>>(key);
        }

        /// <summary>
        /// Get an <see cref="IListState{T}"/> instance
        /// </summary>
        /// <typeparam name="TValue">Data type of state</typeparam>
        /// <param name="unit"><see cref="IUnit"/> to retrieve state for</param>
        /// <param name="key"><see cref="IStateKey"/> to get state list for</param>
        /// <returns><see cref="IListState{T}"/> of <typeparamref name="TValue"/></returns>
        public static IListState<TValue> GetList<TValue>(this IUnit unit, IStateKey key)
        {
            return unit.Get<IListState<TValue>>(key);
        }

        /// <summary>
        /// Get an <see cref="IDictionaryState{TKey, TValue}"/> instance
        /// </summary>
        /// <typeparam name="TKey">Data type of state key</typeparam>
        /// <typeparam name="TValue">Data type of state values</typeparam>
        /// <param name="unit"><see cref="IUnit"/> to retrieve state for</param>
        /// <param name="key"><see cref="IStateKey"/> to get state dictionary for</param>
        /// <returns><see cref="IDictionaryState{TKey, TValue}"/> of <typeparamref name="TKey"/> key and <typeparamref name="TValue"/> value</returns>
        public static IDictionaryState<TKey, TValue> GetDictionary<TKey, TValue>(this IUnit unit, IStateKey key)
            where TKey : IEquatable<TKey>, IComparable<TKey>
        {
            return unit.Get<IDictionaryState<TKey, TValue>>(key);
        }

        /// <summary>
        /// Get an <see cref="IArrayState{T}"/> instance
        /// </summary>
        /// <typeparam name="TValue">Data type of state</typeparam>
        /// <param name="unit"><see cref="IUnit"/> to retrieve state for</param>
        /// <param name="key"><see cref="IStateKey"/> to get state array for</param>
        /// <returns><see cref="IArrayState{T}"/> of <typeparamref name="TValue"/></returns>
        public static IArrayState<TValue> GetArray<TValue>(this IUnit unit, IStateKey key)
        {
            return unit.Get<IArrayState<TValue>>(key);
        }

        /// <summary>
        /// Get an <see cref="IQueueState{T}"/> instance
        /// </summary>
        /// <typeparam name="TValue">Data type of state</typeparam>
        /// <param name="unit"><see cref="IUnit"/> to retrieve state for</param>
        /// <param name="key"><see cref="IStateKey"/> to get state queue for</param>
        /// <returns><see cref="IQueueState{T}"/> of <typeparamref name="TValue"/></returns>
        public static IQueueState<TValue> GetQueue<TValue>(this IUnit unit, IStateKey key)
        {
            return unit.Get<IQueueState<TValue>>(key);
        }

        /// <summary>
        /// Get an <see cref="IStackState{T}"/> instance
        /// </summary>
        /// <typeparam name="TValue">Data type of state</typeparam>
        /// <param name="unit"><see cref="IUnit"/> to retrieve state for</param>
        /// <param name="key"><see cref="IStateKey"/> to get state stack for</param>
        /// <returns><see cref="IStackState{T}"/> of <typeparamref name="TValue"/></returns>
        public static IStackState<TValue> GetStack<TValue>(this IUnit unit, IStateKey key)
        {
            return unit.Get<IStackState<TValue>>(key);
        }

    }
}