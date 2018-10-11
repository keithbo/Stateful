namespace Stateful
{
    public interface IEnumerableState<out T> : IState
    {
        IAsyncEnumerator<T> GetAsyncEnumerator();
    }
}