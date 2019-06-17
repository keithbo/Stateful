namespace Stateful
{
    using System;

    /// <summary>
    /// Unifying interface to identify Stateful state
    /// </summary>
    public interface IStateKey
        : IEquatable<IStateKey>, IComparable<IStateKey>
    {
        /// <summary>
        /// String name of this state key
        /// </summary>
        string Name { get; }
    }
}