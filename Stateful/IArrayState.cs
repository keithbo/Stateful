namespace Stateful
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IArrayState<T> : ICollectionState<T>
    {
        Task<T> GetAsync(long index, CancellationToken cancellationToken = default(CancellationToken));

        Task SetAsync(long index, T value, CancellationToken cancellationToken = default(CancellationToken));
    }
}