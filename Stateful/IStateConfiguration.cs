namespace Stateful
{
    using System;

    public interface IStateConfiguration
    {
        IStateConfiguration Object<TValue>(IStateKey key);

        IStateConfiguration List<TValue>(IStateKey key);

        IStateConfiguration Dictionary<TKey, TValue>(IStateKey key) where TKey : IEquatable<TKey>, IComparable<TKey>;

        IStateConfiguration Array<TValue>(IStateKey key, long length);

        IStateConfiguration Queue<TValue>(IStateKey key);

        IStateConfiguration Stack<TValue>(IStateKey key);


        IStateFactory Build();
    }
}