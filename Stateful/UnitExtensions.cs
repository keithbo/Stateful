namespace Stateful
{
    using System;

    public static class UnitExtensions
    {
        public static IObjectState<TValue> GetObject<TValue>(this IUnit unit, IStateKey key)
        {
            return unit.Get<IObjectState<TValue>>(key);
        }

        public static IListState<TValue> GetList<TValue>(this IUnit unit, IStateKey key)
        {
            return unit.Get<IListState<TValue>>(key);
        }

        public static IDictionaryState<TKey, TValue> GetDictionary<TKey, TValue>(this IUnit unit, IStateKey key)
            where TKey : IEquatable<TKey>, IComparable<TKey>
        {
            return unit.Get<IDictionaryState<TKey, TValue>>(key);
        }

        public static IArrayState<TValue> GetArray<TValue>(this IUnit unit, IStateKey key)
        {
            return unit.Get<IArrayState<TValue>>(key);
        }

        public static IQueueState<TValue> GetQueue<TValue>(this IUnit unit, IStateKey key)
        {
            return unit.Get<IQueueState<TValue>>(key);
        }

        public static IStackState<TValue> GetStack<TValue>(this IUnit unit, IStateKey key)
        {
            return unit.Get<IStackState<TValue>>(key);
        }

    }
}