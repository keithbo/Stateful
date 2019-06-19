namespace Stateful
{
    using System;

    /// <summary>
    /// Unifying interface to identify Stateful state
    /// </summary>
    public interface IStateKey
        : IEquatable<IStateKey>, IComparable<IStateKey>
    {
    }
}