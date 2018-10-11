namespace Stateful
{
    public sealed class ConditionalValue<T>
    {
        public bool HasValue { get; }

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