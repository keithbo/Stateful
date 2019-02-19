namespace Stateful
{
    using System;
    using System.Threading.Tasks;

    public interface IUnit : IDisposable
    {
        TState Get<TState>(IStateKey key) where TState : IState;

        void Abort();

        Task CommitAsync();
    }
}
