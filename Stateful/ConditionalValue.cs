namespace Stateful
{
    /// <summary>
    /// Wraps a value, or tracks that a value doesn't exist
    /// </summary>
    /// <typeparam name="T">Type of the data result</typeparam>
    public sealed class ConditionalValue<T>
    {
        /// <summary>
        /// True if this conditional has a value, False otherwise
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        /// The wrapped value of this conditional
        /// </summary>
        public T Value { get; }

        public ConditionalValue() : this(false, default(T))
        {
        }

        public ConditionalValue(T value) : this(true, value)
        {
        }

        public ConditionalValue(bool hasValue, T value)
        {
            HasValue = hasValue;
            Value = value;
        }
    }
}