namespace Stateful
{
    /// <summary>
    /// State that contains a sequence of values
    /// </summary>
    /// <typeparam name="T">Data type of values</typeparam>
    public interface IEnumerableState<out T> : IState
    {
        /// <summary>
        /// Get an <see cref="IAsyncEnumerator{T}"/> instance to enumerate this state
        /// </summary>
        /// <returns><see cref="IAsyncEnumerator{T}"/></returns>
        IAsyncEnumerator<T> GetAsyncEnumerator();
    }
}