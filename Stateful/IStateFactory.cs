namespace Stateful
{
    using System;

    public interface IStateFactory
    {
        IObjectState<T> CreateObjectState<T>(string name);

        IListState<T> CreateListState<T>(string name);

        IDictionaryState<TKey, TValue> CreateDictionaryState<TKey, TValue>(string name) where TKey : IEquatable<TKey>, IComparable<TKey>;

        IBagState<T> CreateBagState<T>(string name);

        IQueueState<T> CreateQueueState<T>(string name);

        IStackState<T> CreateStackState<T>(string name);
    }
}