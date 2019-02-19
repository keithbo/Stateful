namespace Stateful
{
    using System;

    public interface IStateKey
        : IEquatable<IStateKey>, IComparable<IStateKey>
    {
        string Name { get; }
    }
}