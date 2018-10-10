namespace Stateful
{
    public interface IEnumerableState<T> : IState
    {
        IAsyncEnumerator<T> GetAsyncEnumerator();
    }
}