namespace Stateful
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IState
    {
        string Name { get; }

        Task<bool> HasStateAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task DeleteStateAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
