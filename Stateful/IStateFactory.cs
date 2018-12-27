namespace Stateful
{
    using System;

    public interface IStateFactory
    {
        ITransaction CreateTransaction();

        IObjectState<T> CreateObjectState<T>(string name);

        IListState<T> CreateListState<T>(string name);

        IDictionaryState<TKey, TValue> CreateDictionaryState<TKey, TValue>(string name) where TKey : IEquatable<TKey>, IComparable<TKey>;

        IArrayState<T> CreateArrayState<T>(string name, long length);

        IQueueState<T> CreateQueueState<T>(string name);

        IStackState<T> CreateStackState<T>(string name);
    }
}