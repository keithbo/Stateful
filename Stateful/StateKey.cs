namespace Stateful
{
    using System;

    /// <inheritdoc />
    public sealed class StateKey : IStateKey
    {
        /// <inheritdoc />
        public string Name { get; }

        public StateKey(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <inheritdoc />
        public bool Equals(IStateKey other)
        {
            return EqualsCore(other as StateKey);
        }

        /// <inheritdoc />
        public int CompareTo(IStateKey other)
        {
            return CompareToCore(other as StateKey);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return EqualsCore(obj as StateKey);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }

        internal bool EqualsCore(StateKey other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        internal int CompareToCore(StateKey other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}