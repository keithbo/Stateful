namespace Stateful
{
    using System;
    using System.Threading.Tasks;

    public interface ITransaction : IDisposable
    {
        void Abort();

        Task CommitAsync();
    }
}
