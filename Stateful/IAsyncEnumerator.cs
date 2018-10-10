namespace Stateful
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAsyncEnumerator<out T> : IDisposable
    {
        T Current { get; }

        Task<bool> MoveNextAsync(CancellationToken cancellationToken = default(CancellationToken));

        void Reset();
    }
}